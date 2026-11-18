using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using DC_bot.Constants;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.Discovery;
using DC_bot.Interface.Service.Music.Events;
using DC_bot.Interface.Service.Music.Lifecycle;
using DC_bot.Interface.Service.Music.Playback;
using DC_bot.Interface.Service.Music.Playback.Models;
using DC_bot.Interface.Service.Music.Search;
using DC_bot.Interface.Service.Music.Tracks;
using DC_bot.Interface.Service.Persistence.Discovery;
using DC_bot.Service.Music.Lifecycle.Sessions;
using DC_bot.Wrapper.Lavalink;
using Lavalink4NET;
using Lavalink4NET.Rest.Entities.Tracks;

namespace DC_bot.Service.Music.Discovery;

/// <summary>Manages user-owned favorites while reusing the existing source and playback workflows.</summary>
/// <param name="repository">Persists owner-scoped favorites.</param>
/// <param name="audio">Loads optional single-track source queries.</param>
/// <param name="resolver">Resolves source search modes.</param>
/// <param name="serializer">Serializes stable source tracks.</param>
/// <param name="sessions">Validates player generations.</param>
/// <param name="connection">Joins or resolves a player for explicit favorite playback.</param>
/// <param name="events">Registers playback notifications.</param>
/// <param name="playback">Enqueues and starts through the canonical playback pipeline.</param>
/// <param name="recommendations">Resets automatic continuation on a manual request.</param>
/// <param name="localization">Formats guild-language responses.</param>
public sealed class FavoriteService(
    IFavoriteRepository repository,
    IAudioService audio,
    ITrackSearchResolverService resolver,
    ITrackSerializer serializer,
    GuildPlaybackSessions sessions,
    IPlayerConnectionService connection,
    IPlaybackEventHandlerService events,
    ITrackPlaybackService playback,
    IRecommendationService recommendations,
    ILocalizationService localization) : IFavoriteService
{

    /// <inheritdoc />
    public async Task<PlaybackControlResult> ExecuteAsync(
        IDiscordMessage message,
        string arguments,
        CancellationToken cancellationToken = default)
    {
        var guildId = message.Channel.Guild.Id;
        var userId = message.Author.Id;
        var parts = arguments.Split((char[]?)null, 2, StringSplitOptions.RemoveEmptyEntries);
        var operation = parts.Length == 0 ? "list" : parts[0].ToLowerInvariant();
        var argument = parts.Length == 2 ? parts[1].Trim() : string.Empty;

        if (operation == "list")
        {
            var page = 1;

            if (argument.Length > 0
                && (!int.TryParse(argument, NumberStyles.None, CultureInfo.InvariantCulture, out page)
                    || page is < 1 or > 1000000))
            {
                return Fail(guildId, LocalizationKeys.DiscoveryInvalid);
            }

            var items = await repository.ListAsync(userId, page, cancellationToken);
            var lines = string.Join("\n", items.Select(item =>
                $"{item.Id}. {Bound(item.Author)} - {Bound(item.Title)}"));
            return PlaybackControlResult.Succeeded(localization.Get(guildId,
                items.Count == 0 ? LocalizationKeys.FavoriteEmpty : LocalizationKeys.FavoriteList, page, lines));
        }

        if (operation == "add")
        {
            var track = sessions.CaptureCurrent(guildId)?.Player.CurrentTrack;

            if (argument.Length > 0)
            {
                var loaded = await audio.Tracks.LoadTracksAsync(argument, resolver.ResolveSearchMode(argument), cancellationToken: cancellationToken);
                track = loaded.IsPlaylist ? null : loaded.Track;
            }

            if (track is null)
            {
                return Fail(guildId, LocalizationKeys.DiscoveryNoTrack);
            }

            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(track.SourceName + ":" + track.Identifier)));
            var item = await repository.AddAsync(userId, key, serializer.Serialize(new LavaLinkTrackWrapper(track)),
                track.Title, track.Author, cancellationToken);
            return PlaybackControlResult.Succeeded(localization.Get(guildId, LocalizationKeys.FavoriteAdded, item.Id, Bound(item.Title)));
        }

        if (!long.TryParse(argument, NumberStyles.None, CultureInfo.InvariantCulture, out var id)
            || id <= 0)
        {
            return Fail(guildId, LocalizationKeys.DiscoveryInvalid);
        }

        if (operation == "remove")
        {
            return await repository.RemoveAsync(userId, id, cancellationToken)
                ? PlaybackControlResult.Succeeded(localization.Get(guildId, LocalizationKeys.FavoriteRemoved, id))
                : Fail(guildId, LocalizationKeys.FavoriteMissing);
        }

        if (operation != "play")
        {
            return Fail(guildId, LocalizationKeys.DiscoveryInvalid);
        }

        var favorite = await repository.GetAsync(userId, id, cancellationToken);

        if (favorite is null)
        {
            return Fail(guildId, LocalizationKeys.FavoriteMissing);
        }

        var guild = message.Channel.Guild;
        var member = await guild.GetMemberAsync(userId)
            .WaitAsync(cancellationToken);
        var voice = member.VoiceState?.Channel;
        var current = sessions.CaptureCurrent(guildId);

        if (voice is null
            || (current is not null && current.Player.VoiceChannelId != voice.Id))
        {
            return Fail(guildId, LocalizationKeys.PlaybackWrongVoiceChannel);
        }

        var voiceWrapper = voice;
        var (player, _, _, valid) = await connection.TryJoinAndValidateAsync(message, voiceWrapper, cancellationToken);
        var session = valid && player is not null ? sessions.Capture(guildId, player) : null;

        if (session is null || player!.VoiceChannelId != voice.Id)
        {
            return Fail(guildId, ValidationErrorKeys.BotIsNotConnectedError);
        }

        events.RegisterPlaybackFinishedHandler(guildId, player, message.Channel);
        recommendations.ManualRequest(guildId);
        var restored = serializer.Deserialize(favorite.TrackIdentifier);
        var result = await playback.PlayTheFoundMusicAsync(TrackLoadResult.CreateTrack(restored.ToLavalinkTrack()),
            session, message.Channel, $"<@{message.Author.Id}>", restored.ToLavalinkTrack().Uri?.ToString(),
            TrackSearchMode.None, cancellationToken);
        return result.Status switch
        {
            PlaybackStartStatus.Started or PlaybackStartStatus.Queued => PlaybackControlResult.Succeeded(
                localization.Get(guildId, LocalizationKeys.FavoritePlaying, id)),
            PlaybackStartStatus.QueueFull => Fail(guildId, ValidationErrorKeys.QueueCapacityExceeded),
            PlaybackStartStatus.StartedStatePending => Fail(guildId, ValidationErrorKeys.PlaybackStatePending),
            PlaybackStartStatus.Unknown => Fail(guildId, ValidationErrorKeys.PlaybackOutcomeUnknown),
            PlaybackStartStatus.Superseded => Fail(guildId, ValidationErrorKeys.PlaybackSuperseded),
            PlaybackStartStatus.Busy => Fail(guildId, ValidationErrorKeys.PlaybackBusy),
            _ => Fail(guildId, ValidationErrorKeys.LavalinkError)
        };
    }

    /// <summary>Bounds user-facing provider metadata for Discord message limits.</summary>
    private static string Bound(string value) => value.Length > 65 ? value[..62] + "..." : value;

    /// <summary>Creates a localized failure result.</summary>
    private PlaybackControlResult Fail(ulong guildId, string key) =>
        PlaybackControlResult.Failed(localization.Get(guildId, key), key);
}
