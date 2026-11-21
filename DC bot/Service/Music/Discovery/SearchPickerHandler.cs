using System.Collections.Concurrent;
using System.Globalization;
using DC_bot.Constants;
using DC_bot.Helper;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.Discovery;
using DC_bot.Interface.Service.Music.Playback.Models;
using DC_bot.Interface.Service.Persistence.Discovery;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Service.Music.Lifecycle.Models;
using DC_bot.Service.Music.Lifecycle.Sessions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Lavalink4NET.Rest.Entities.Tracks;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.Discovery;

/// <summary>Owns expiring, single-use Discord search menus scoped to user, guild, message and player generation.</summary>
/// <param name="settings">Persists personal opt-in preferences.</param>
/// <param name="sessions">Validates playback ownership after a delayed selection.</param>
/// <param name="localization">Formats all menu text through existing catalogs.</param>
/// <param name="logger">Records failed Discord presentation or interaction acknowledgement.</param>
/// <param name="responseBuilder">Creates localized embeds through the shared presentation boundary.</param>
public sealed class SearchPickerHandler(
    IDiscoverySettingsRepository settings,
    GuildPlaybackSessions sessions,
    ILocalizationService localization,
    ILogger<SearchPickerHandler> logger,
    IResponseBuilder responseBuilder) : ISearchPickerHandler
{
    private const string Prefix = "music-search:";
    private readonly ConcurrentDictionary<string, PendingChoice> _pending = new();
    private readonly ConcurrentDictionary<(ulong Guild, ulong User), string> _latest = new();

    /// <summary>Retains one bounded live menu and its completion, without persistent SDK objects.</summary>
    private sealed class PendingChoice
    {
        public required ulong UserId;
        public required PlaybackSession Session;
        public required LavalinkTrack[] Tracks;
        public required DateTimeOffset Expires;
        public readonly TaskCompletionSource<LavalinkTrack?> Completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public DiscordMessage? Message;
    }

    /// <inheritdoc />
    public void CancelPending(ulong guildId, ulong userId)
    {
        if (_latest.TryRemove((guildId, userId), out var nonce)
            && _pending.TryGetValue(nonce, out var pending))
        {
            pending.Completion.TrySetResult(null);
        }
    }

    /// <inheritdoc />
    public async Task<PlaybackControlResult> ConfigureAsync(
        IDiscordMessage message,
        string arguments,
        CancellationToken token = default)
    {
        var value = arguments.Trim().ToLowerInvariant();

        if (value is not ("" or "status" or "on" or "off"))
        {
            return PlaybackControlResult.Failed(localization.Get(message.Channel.Guild.Id, LocalizationKeys.DiscoveryInvalid),
                LocalizationKeys.DiscoveryInvalid);
        }

        if (value is "on" or "off")
        {
            await settings.SetPickerAsync(message.Author.Id, value == "on", token);
        }

        var enabled = await settings.GetPickerAsync(message.Author.Id, token);
        return PlaybackControlResult.Succeeded(localization.Get(message.Channel.Guild.Id,
            enabled ? LocalizationKeys.SearchPickerEnabled : LocalizationKeys.SearchPickerDisabled)) with
        {
            IsInformational = value is "" or "status",
            ResponseTitleKey = value is "" or "status" ? null
                : enabled ? LocalizationKeys.ResponseOutcomeSearchPickerEnabledTitle
                : LocalizationKeys.ResponseOutcomeSearchPickerDisabledTitle
        };
    }

    /// <inheritdoc />
    public async Task<LavalinkTrack?> SelectAsync(
        IDiscordMessage message,
        PlaybackSession session,
        TrackLoadResult loaded,
        CancellationToken token = default)
    {
        if (!await settings.GetPickerAsync(message.Author.Id, token))
        {
            return loaded.Track;
        }

        var tracks = loaded.Tracks.Take(5)
            .ToArray();

        if (tracks.Length == 0)
        {
            return null;
        }

        var guildId = session.GuildId;
        var nonce = Guid.NewGuid().ToString("N");
        var key = (guildId, message.Author.Id);
        var pending = new PendingChoice
        {
            UserId = message.Author.Id, Session = session, Tracks = tracks,
            Expires = DateTimeOffset.UtcNow.AddSeconds(45)
        };
        _latest.AddOrUpdate(key, nonce, (_, old) =>
        {
            if (_pending.TryGetValue(old, out var superseded))
            {
                superseded.Completion.TrySetResult(null);
            }

            return nonce;
        });
        _pending[nonce] = pending;
        var selected = false;
        try
        {
            var options = CreateOptions(tracks);
            var menu = new DiscordSelectComponent(Prefix + nonce,
                localization.Get(guildId, LocalizationKeys.SearchPickerPlaceholder), options);
            var rows = string.Join("\n", tracks.Select((track, index) =>
                $"**{index + 1}.** {DC_bot.Helper.DiscordTextSanitizer.EscapeMentions(track.Title)}"));
            var prompt = localization.Get(guildId, LocalizationKeys.SearchPickerDesignBody, rows);
            var channel = message.Channel.ToDiscordChannel();
            token.ThrowIfCancellationRequested();

            try
            {
                pending.Message = await channel.SendMessageAsync(new DiscordMessageBuilder()
                    .AddEmbed(responseBuilder.CreateEmbed(guildId, prompt, titleKey: LocalizationKeys.SearchPickerDesignTitle))
                    .AddActionRowComponent(menu));
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                token.ThrowIfCancellationRequested();
                logger.LogDebug(exception, "Picker embed failed; retrying with text. Guild={GuildId}", guildId);
                pending.Message = await channel.SendMessageAsync(new DiscordMessageBuilder()
                    .WithContent(DiscordTextSanitizer.Truncate(prompt, 2000))
                    .AddActionRowComponent(menu));
            }

            token.ThrowIfCancellationRequested();

            if (_latest.GetValueOrDefault(key) != nonce)
            {
                pending.Completion.TrySetResult(null);
            }

            var choice = await pending.Completion.Task.WaitAsync(TimeSpan.FromSeconds(45), token);

            if (choice is null
                || !sessions.IsCurrent(session)
                || _latest.GetValueOrDefault(key) != nonce)
            {
                return null;
            }

            var member = await message.Channel.Guild.GetMemberAsync(message.Author.Id)
                .WaitAsync(token);

            if (member.VoiceState?.Channel?.Id != session.Player.VoiceChannelId)
            {
                return null;
            }

            selected = true;
            return choice;
        }
        catch (TimeoutException)
        {
            return null;
        }
        finally
        {
            _pending.TryRemove(nonce, out _);
            ((ICollection<KeyValuePair<(ulong Guild, ulong User), string>>)_latest)
                .Remove(new(key, nonce));

            if (pending.Message is not null)
            {
                try
                {
                    var closed = localization.Get(guildId,
                        selected ? LocalizationKeys.SearchPickerSelected : LocalizationKeys.SearchPickerClosed);

                    try
                    {
                        await pending.Message.ModifyAsync(new DiscordMessageBuilder()
                            .AddEmbed(responseBuilder.CreateEmbed(guildId, closed)));
                    }
                    catch (Exception exception) when (exception is not OperationCanceledException)
                    {
                        logger.LogDebug(exception, "Picker cleanup embed failed; retrying with text. Guild={GuildId}", guildId);
                        await pending.Message.ModifyAsync(new DiscordMessageBuilder()
                            .WithContent(DiscordTextSanitizer.Truncate(closed, 2000)));
                    }
                }
                catch (Exception exception)
                {
                    logger.LogDebug(exception, "Picker cleanup failed. Guild={GuildId}", guildId);
                }
            }
        }
    }

    /// <inheritdoc />
    public async Task OnComponentInteractionAsync(DiscordClient client, ComponentInteractionCreatedEventArgs args)
    {
        if (!args.Id.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return;
        }

        var value = args.Id[Prefix.Length..];
        var nonce = value.Split(':')[0];

        if (!_pending.TryGetValue(nonce, out var pending)
            || pending.Expires <= DateTimeOffset.UtcNow
            || args.Guild?.Id != pending.Session.GuildId
            || args.User.Id != pending.UserId
            || args.Message.Id != pending.Message?.Id
            || !sessions.IsCurrent(pending.Session))
        {
            await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                    .AddEmbed(responseBuilder.CreateEmbed(args.Guild?.Id,
                        localization.Get(args.Guild?.Id ?? 0, LocalizationKeys.SearchPickerDenied),
                        DiscordColor.Gold, LocalizationKeys.ResponseWarningTitle))
                    .AsEphemeral());
            return;
        }

        var member = await args.Guild.GetMemberAsync(args.User.Id);

        if (member.VoiceState?.ChannelId != pending.Session.Player.VoiceChannelId)
        {
            await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                    .AddEmbed(responseBuilder.CreateEmbed(args.Guild.Id,
                        localization.Get(args.Guild.Id, LocalizationKeys.PlaybackWrongVoiceChannel),
                        DiscordColor.Red, LocalizationKeys.ResponseErrorTitle))
                    .AsEphemeral());
            return;
        }

        await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);

        if (args.Values.Length == 1
            && int.TryParse(args.Values[0], NumberStyles.None, CultureInfo.InvariantCulture, out var index)
            && index >= 0
            && index < pending.Tracks.Length)
        {
            pending.Completion.TrySetResult(pending.Tracks[index]);
        }
    }

    /// <summary>Bounds provider metadata to Discord component field limits.</summary>
    private static string Bound(string value, int length) => value.Length > length ? value[..(length - 3)] + "..." : value;

    /// <summary>Creates at most five real, bounded menu options with stable indices into the captured source results.</summary>
    internal static IReadOnlyList<DiscordSelectComponentOption> CreateOptions(IReadOnlyList<LavalinkTrack> tracks)
    {
        return tracks.Take(5)
            .Select((track, index) => new DiscordSelectComponentOption(
                Bound($"{index + 1}. {track.Author} - {track.Title}", 100),
                index.ToString(CultureInfo.InvariantCulture), Bound(track.Duration.ToString(), 100)))
            .ToArray();
    }
}
