using System.Collections.Concurrent;
using System.Globalization;
using DC_bot.Constants;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.Playback.Models;
using DC_bot.Interface.Service.Persistence.Guilds;
using DC_bot.Interface.Service.Persistence.MusicControl;
using DC_bot.Interface.Service.Persistence.Queue;
using DC_bot.Interface.Service.Music.Presentation;
using DC_bot.Service.BotControl;
using DC_bot.Service.Music.Lifecycle.Models;
using DC_bot.Service.Music.Lifecycle.PersistentVoice;
using DC_bot.Service.Music.Lifecycle.Sessions;
using DC_bot.Wrapper.Discord;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.Controls;

/// <summary>Coordinates durable guild controls, track-scoped votes, empty-channel leave and read-only status.</summary>
/// <param name="settings">Stores guild opt-in voting and DJ roles.</param>
/// <param name="guildSettings">Reads the independent 24/7 preference.</param>
/// <param name="queue">Reads authoritative requester and waiting-track information.</param>
/// <param name="sessions">Owns player and track-attempt identity.</param>
/// <param name="client">Provides the current gateway voice membership.</param>
/// <param name="leave">Executes the established full leave path.</param>
/// <param name="localization">Resolves all user-visible text.</param>
/// <param name="logger">Records background control and notification failures.</param>
/// <param name="notifications">Owns background response styling, delivery and text fallback.</param>
public sealed class MusicControlSettingsService(
    IGuildMusicControlSettingsRepository settings,
    IGuildDataRepository guildSettings,
    IQueueRepository queue,
    GuildPlaybackSessions sessions,
    DiscordClient client,
    PersistentVoiceLeaveService leave,
    ILocalizationService localization,
    ILogger<MusicControlSettingsService> logger,
    ITrackNotificationService notifications)
{
    private readonly ConcurrentDictionary<ulong, VoteState> _votes = new();
    private readonly ConcurrentDictionary<ulong, (long Generation, DateTimeOffset Since)> _empty = new();

    /// <summary>Holds votes for exactly one ready attempt; the monitor protects maintenance reads.</summary>
    private sealed class VoteState(long generation, Guid attempt)
    {
        public readonly object Sync = new();
        public readonly long Generation = generation;
        public readonly Guid Attempt = attempt;
        public readonly HashSet<ulong> Users = [];
    }

    /// <summary>Cancels an empty-channel countdown immediately when persistent voice is enabled.</summary>
    public void CancelEmptyCountdown(ulong guildId)
    {
        _empty.TryRemove(guildId, out _);
    }

    /// <summary>Observes gateway membership so even a brief returning listener cancels the countdown.</summary>
    public void OnVoiceStateChanged(ulong guildId, ulong userId, ulong? beforeChannelId, ulong? afterChannelId)
    {
        var session = sessions.CaptureCurrent(guildId);

        if (session is null)
        {
            _empty.TryRemove(guildId, out _);
            _votes.TryRemove(guildId, out _);
            return;
        }

        if (afterChannelId == session.Player.VoiceChannelId
            && client.Guilds.TryGetValue(guildId, out var guild)
            && (!guild.Members.TryGetValue(userId, out var member) || !member.IsBot))
        {
            _empty.TryRemove(guildId, out _);
        }

        if (beforeChannelId == session.Player.VoiceChannelId
            && afterChannelId != beforeChannelId
            && _votes.TryGetValue(guildId, out var votes))
        {
            lock (votes.Sync)
            {
                votes.Users.Remove(userId);
            }
        }
    }

    /// <summary>Computes the required half-room threshold, rounding odd listener counts upward.</summary>
    internal static int RequiredVotes(int humanCount) => Math.Max(1, (humanCount + 1) / 2);

    /// <summary>Matches only an unambiguous Discord mention; legacy display names grant no ownership.</summary>
    internal static bool IsRequester(string? requestedBy, ulong userId)
    {
        return requestedBy == $"<@{userId}>" || requestedBy == $"<@!{userId}>";
    }

    /// <summary>Decides direct skip access without granting any unrelated DJ privileges.</summary>
    internal static bool CanSkipWithoutVoting(
        bool enabled,
        string? requestedBy,
        ulong userId,
        ulong? djRoleId,
        IEnumerable<ulong> memberRoleIds)
    {
        return !enabled || IsRequester(requestedBy, userId)
            || (djRoleId.HasValue && memberRoleIds.Contains(djRoleId.Value));
    }

    /// <summary>Checks and records a vote under the caller's exclusive skip reservation.</summary>
    /// <returns>Null when skip is approved; otherwise a localized response without advancing playback.</returns>
    public async Task<PlaybackControlResult?> EvaluateSkipAsync(
        PlaybackSession session,
        IDiscordMessage message,
        IDiscordMember? member,
        CancellationToken cancellationToken,
        bool requireVoting = false)
    {
        var config = await settings.GetAsync(session.GuildId, cancellationToken);

        if (!config.VoteSkipEnabled)
        {
            _votes.TryRemove(session.GuildId, out _);
            return requireVoting
                ? PlaybackControlResult.Failed("Voting disabled.", ValidationErrorKeys.PlaybackSuperseded) : null;
        }

        var humans = await GetListenersAsync(session, cancellationToken);

        if (member is null || !humans.ContainsKey(member.Id))
        {
            return PlaybackControlResult.Failed("Not a current listener.", LocalizationKeys.PlaybackWrongVoiceChannel);
        }

        var attempt = sessions.GetReadyAttemptId(session);
        var itemId = sessions.GetReadyQueueItemId(session);

        if (attempt is null || itemId is null)
        {
            return PlaybackControlResult.Failed("No ready track.", LocalizationKeys.SkipCommandError);
        }

        var item = await queue.GetByIdAsync(itemId.Value, cancellationToken);
        var actor = await message.Channel.Guild.GetMemberAsync(member.Id).WaitAsync(cancellationToken);
        var direct = CanSkipWithoutVoting(config.VoteSkipEnabled,
            item?.GuildId == session.GuildId ? item.RequestedBy : null, member.Id, config.DjRoleId,
            actor.ToDiscordMember().Roles.Select(role => role.Id));

        if (!sessions.IsCurrent(session)
            || sessions.GetReadyAttemptId(session) != attempt)
        {
            return PlaybackControlResult.Failed("Track changed.", ValidationErrorKeys.PlaybackSuperseded);
        }

        if (direct)
        {
            return null;
        }

        var (count, required) = RegisterVote(session, attempt.Value, humans.Keys, member.Id);
        return count >= required ? null : PlaybackControlResult.Succeeded(localization.Get(
            session.GuildId, LocalizationKeys.MusicVoteProgress, count, required));
    }

    /// <summary>Records one eligible vote per user and discards votes from another attempt or departed listeners.</summary>
    internal (int Count, int Required) RegisterVote(
        PlaybackSession session,
        Guid attempt,
        IEnumerable<ulong> listeners,
        ulong voter)
    {
        var humans = listeners.ToHashSet();
        var votes = _votes.AddOrUpdate(session.GuildId,
            _ => new VoteState(session.Generation, attempt),
            (_, old) => old.Generation == session.Generation && old.Attempt == attempt
                ? old : new VoteState(session.Generation, attempt));
        lock (votes.Sync)
        {
            votes.Users.IntersectWith(humans);

            if (humans.Contains(voter))
            {
                votes.Users.Add(voter);
            }

            return (votes.Users.Count, RequiredVotes(humans.Count));
        }
    }

    /// <summary>Protects direct queue jumps from bypassing voting without turning a vote into a multi-track jump.</summary>
    public async Task<bool> CanSkipDirectlyAsync(
        PlaybackSession session,
        IDiscordMessage message,
        IDiscordMember? member,
        CancellationToken token)
    {
        var config = await settings.GetAsync(session.GuildId, token);

        if (!config.VoteSkipEnabled)
        {
            return true;
        }

        if (member is null)
        {
            return false;
        }

        var actor = await message.Channel.Guild.GetMemberAsync(member.Id).WaitAsync(token);

        if (config.DjRoleId is { } roleId
            && actor.ToDiscordMember().Roles.Any(role => role.Id == roleId))
        {
            return true;
        }

        var itemId = sessions.GetReadyQueueItemId(session);
        var item = itemId.HasValue ? await queue.GetByIdAsync(itemId.Value, token) : null;
        return item?.GuildId == session.GuildId && IsRequester(item.RequestedBy, member.Id);
    }

    /// <summary>Reads or changes guild settings; mutations require Manage Guild but do not require an active player.</summary>
    public async Task<PlaybackControlResult> ConfigureAsync(
        IDiscordMessage message,
        string arguments,
        CancellationToken cancellationToken = default)
    {
        var guildId = message.Channel.Guild.Id;
        var parts = arguments.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0 || parts is ["show"])
        {
            var current = await settings.GetAsync(guildId, cancellationToken);
            return PlaybackControlResult.Information(localization.Get(guildId, LocalizationKeys.MusicConfigResponse,
                Enabled(guildId, current.VoteSkipEnabled), Role(guildId, current.DjRoleId)));
        }

        var member = await message.Channel.Guild.GetMemberAsync(message.Author.Id).WaitAsync(cancellationToken);
        var permissions = member.ToDiscordMember().Permissions;

        if (!permissions.HasPermission(DiscordPermission.ManageGuild)
            && !permissions.HasPermission(DiscordPermission.Administrator))
        {
            return PlaybackControlResult.Failed("Manage Guild required.", LocalizationKeys.MusicConfigForbidden);
        }

        if (parts.Length != 2)
        {
            return PlaybackControlResult.Failed("Invalid settings.", LocalizationKeys.MusicConfigUsage);
        }

        if (parts[0].Equals("vote-skip", StringComparison.OrdinalIgnoreCase)
            && TrySwitch(parts[1], out var enabled))
        {
            await settings.SetVoteSkipAsync(guildId, enabled, cancellationToken);
            _votes.TryRemove(guildId, out _);
        }
        else if (parts[0].Equals("dj-role", StringComparison.OrdinalIgnoreCase))
        {
            ulong? roleId = null;

            if (!parts[1].Equals("clear", StringComparison.OrdinalIgnoreCase))
            {
                var value = parts[1].Trim('<', '@', '&', '>');

                if (!ulong.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed)
                    || !client.Guilds.TryGetValue(guildId, out var guild)
                    || parsed == guildId
                    || !guild.Roles.ContainsKey(parsed))
                {
                    return PlaybackControlResult.Failed("Unknown role.", LocalizationKeys.MusicConfigUsage);
                }

                roleId = parsed;
            }

            await settings.SetDjRoleAsync(guildId, roleId, cancellationToken);
        }
        else
        {
            return PlaybackControlResult.Failed("Invalid settings.", LocalizationKeys.MusicConfigUsage);
        }

        var result = await ConfigureAsync(message, "show", cancellationToken);
        return result with { IsInformational = false };
    }

    /// <summary>Performs one maintenance pass using the existing five-second recovery worker lifetime.</summary>
    public async Task MaintainAsync(
        ulong guildId,
        Func<IDiscordMessage, IDiscordMember, Guid, Task<PlaybackControlResult>> skip,
        CancellationToken cancellationToken)
    {
        var session = sessions.CaptureCurrent(guildId);

        if (session is null)
        {
            _votes.TryRemove(guildId, out _);
            _empty.TryRemove(guildId, out _);
            return;
        }

        var config = await guildSettings.GetTwentyFourSevenSettingsAsync(guildId, cancellationToken);
        var humans = await GetListenersAsync(session, cancellationToken);
        var channels = sessions.GetRecoveryChannels(guildId);

        if (config.IsEnabled || humans.Count > 0)
        {
            _empty.TryRemove(guildId, out _);
        }
        else
        {
            var deadline = _empty.AddOrUpdate(guildId,
                _ => (session.Generation, DateTimeOffset.UtcNow),
                (_, old) => old.Generation == session.Generation ? old : (session.Generation, DateTimeOffset.UtcNow));

            if (DateTimeOffset.UtcNow - deadline.Since >= TimeSpan.FromMinutes(5))
            {
                var result = await leave.LeaveIfEligibleAsync(guildId, async token =>
                    sessions.IsCurrent(session)
                        && _empty.TryGetValue(guildId, out var activeDeadline)
                        && activeDeadline == deadline
                        && (await GetListenersAsync(session, token)).Count == 0,
                    cancellationToken);

                if (result is not null)
                {
                    _votes.TryRemove(guildId, out _);
                    _empty.TryRemove(guildId, out _);

                    logger.LogInformation("Empty-channel leave completed. Guild={GuildId} Complete={Complete}",
                        guildId, result.FullySucceeded);

                    if (result.FullySucceeded && channels is { } remembered)
                    {
                        var channel = await client.GetChannelAsync(remembered.TextChannelId).WaitAsync(cancellationToken);
                        await notifications.NotifyMusicControlAsync(channel, guildId,
                            localization.Get(guildId, LocalizationKeys.MusicEmptyLeave), false, cancellationToken);
                    }

                    return;
                }
            }
        }

        if (!_votes.TryGetValue(guildId, out var votes))
        {
            return;
        }

        ulong voter;
        lock (votes.Sync)
        {
            if (votes.Generation != session.Generation
                || votes.Attempt != sessions.GetReadyAttemptId(session))
            {
                ((ICollection<KeyValuePair<ulong, VoteState>>)_votes).Remove(new(guildId, votes));
                return;
            }

            votes.Users.IntersectWith(humans.Keys);

            if (votes.Users.Count < RequiredVotes(humans.Count))
            {
                return;
            }

            voter = votes.Users.First();
        }

        if (channels is not { } target)
        {
            return;
        }

        var text = await client.GetChannelAsync(target.TextChannelId).WaitAsync(cancellationToken);
        var actor = humans[voter];
        var message = new BotControlDiscordMessage(new DiscordChannelWrapper(text), new DiscordUserWrapper(actor));
        var outcome = await skip(message, new DiscordMemberWrapper(actor), votes.Attempt);

        if (outcome.Success)
        {
            await notifications.NotifyMusicControlAsync(text, guildId, outcome.Message, true, cancellationToken);
        }
    }

    /// <summary>Returns a localized read-only view without requiring a voice connection.</summary>
    public async Task<PlaybackControlResult> StatusAsync(IDiscordMessage message, CancellationToken cancellationToken = default)
    {
        var guildId = message.Channel.Guild.Id;
        var session = sessions.CaptureCurrent(guildId);
        var waiting = await queue.GetQueuedItemsAsync(guildId, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        if (session is not null && !sessions.IsCurrent(session))
        {
            return PlaybackControlResult.Failed("Session changed.", ValidationErrorKeys.PlaybackSuperseded);
        }

        var healthKey = LocalizationKeys.MusicStatusUnchecked;

        if (session is not null)
        {
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            deadline.CancelAfter(TimeSpan.FromSeconds(3));

            try
            {
                await session.Player.ApiClient.RetrieveServerInformationAsync(deadline.Token).AsTask().WaitAsync(deadline.Token);
                healthKey = LocalizationKeys.MusicStatusReady;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogDebug(exception, "Lavalink health query failed. Guild={GuildId}", guildId);
                healthKey = LocalizationKeys.MusicStatusUnavailable;
            }

            if (!sessions.IsCurrent(session))
            {
                return PlaybackControlResult.Failed("Session changed.", ValidationErrorKeys.PlaybackSuperseded);
            }
        }

        return PlaybackControlResult.Information(localization.Get(guildId, LocalizationKeys.MusicStatusDesignBody,
            localization.Get(guildId, "music_connection_" + sessions.GetConnectionPhase(guildId)),
            localization.Get(guildId, healthKey), waiting.Count));
    }

    /// <summary>Reads human voice participants from the current gateway snapshot; unknown guild state fails closed.</summary>
    private async Task<Dictionary<ulong, DiscordMember>> GetListenersAsync(PlaybackSession session, CancellationToken token)
    {
        if (!client.Guilds.TryGetValue(session.GuildId, out var guild))
        {
            throw new InvalidOperationException("Guild voice state unavailable.");
        }

        var result = new Dictionary<ulong, DiscordMember>();

        foreach (var entry in guild.VoiceStates)
        {
            if (entry.Value.ChannelId != session.Player.VoiceChannelId)
            {
                continue;
            }

            var member = guild.Members.GetValueOrDefault(entry.Key)
                ?? await guild.GetMemberAsync(entry.Key).WaitAsync(token);

            if (!member.IsBot)
            {
                result[member.Id] = member;
            }
        }

        return result;
    }

    /// <summary>Formats a boolean with the guild's catalog.</summary>
    private string Enabled(ulong guildId, bool enabled) => localization.Get(guildId, enabled ? "music_enabled" : "music_disabled");

    /// <summary>Formats an optional configured role.</summary>
    private string Role(ulong guildId, ulong? roleId) => roleId is { } id ? $"<@&{id}>"
        : localization.Get(guildId, LocalizationKeys.PlaybackDetailsNone);

    /// <summary>Accepts explicit text and slash boolean spellings.</summary>
    private static bool TrySwitch(string value, out bool enabled)
    {
        enabled = value.Equals("on", StringComparison.OrdinalIgnoreCase)
            || value.Equals("true", StringComparison.OrdinalIgnoreCase);
        return enabled || value.Equals("off", StringComparison.OrdinalIgnoreCase)
            || value.Equals("false", StringComparison.OrdinalIgnoreCase);
    }
}
