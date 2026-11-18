using System.Collections.Concurrent;
using DC_bot.Constants;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.Discovery;
using DC_bot.Interface.Service.Music.Playback.Models;
using DC_bot.Interface.Service.Music.Search;
using DC_bot.Interface.Service.Music.Tracks;
using DC_bot.Interface.Service.Persistence.Discovery;
using DC_bot.Interface.Service.Persistence.Models.Queue;
using DC_bot.Interface.Service.Persistence.Queue;
using DC_bot.Service.Music.Lifecycle.Models;
using DC_bot.Service.Music.Lifecycle.Sessions;
using DC_bot.Wrapper.Lavalink;
using Lavalink4NET;
using Lavalink4NET.Rest.Entities.Tracks;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.Discovery;

/// <summary>Loads provider radio/mix recommendations and bounds automatic queue continuation.</summary>
/// <param name="audio">Loads radio/mix sources through the configured Lavalink node.</param>
/// <param name="resolver">Resolves optional recommendation seed queries.</param>
/// <param name="serializer">Restores persisted source identities.</param>
/// <param name="settings">Persists the independent opt-in autoplay setting.</param>
/// <param name="queue">Provides durable waiting state and conditional enqueue.</param>
/// <param name="sessions">Fences player generations and automatic queue decisions.</param>
/// <param name="localization">Formats responses through the existing catalogs.</param>
/// <param name="logger">Records provider failures and unavailable automatic continuation.</param>
public sealed class RecommendationService(
    IAudioService audio,
    ITrackSearchResolverService resolver,
    ITrackSerializer serializer,
    IDiscoverySettingsRepository settings,
    IQueueRepository queue,
    GuildPlaybackSessions sessions,
    ILocalizationService localization,
    ILogger<RecommendationService> logger) : IRecommendationService
{
    private const string AutomaticRequester = "autoplay";
    private readonly ConcurrentDictionary<ulong, GuildState> _states = new();

    /// <summary>Retains a bounded process-local recent history, manual revision and automatic chain count.</summary>
    private sealed class GuildState
    {
        public readonly object Sync = new();
        public readonly Queue<(string Source, string Name)> Recent = new();
        public long Revision;
        public int AutomaticCount;
    }

    /// <inheritdoc />
    public void ManualRequest(ulong guildId)
    {
        var state = _states.GetOrAdd(guildId, _ => new());
        lock (state.Sync)
        {
            state.Revision++;
            state.AutomaticCount = 0;
        }
    }

    /// <inheritdoc />
    public void ObserveEnded(ulong guildId, QueueItemRecord item)
    {
        if (!string.Equals(item.RequestedBy, AutomaticRequester, StringComparison.Ordinal))
        {
            // Shared playlist/mobile enqueue paths may bypass the Discord source-request adapter.
            ManualRequest(guildId);
        }

        try
        {
            var track = serializer.Deserialize(item.TrackIdentifier).ToLavalinkTrack();
            var state = _states.GetOrAdd(guildId, _ => new());
            lock (state.Sync)
            {
                state.Recent.Enqueue((Key(track), Name(track)));

                while (state.Recent.Count > 20)
                {
                    state.Recent.Dequeue();
                }
            }
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Autoplay recent identity unavailable. Guild={GuildId}", guildId);
        }
    }

    /// <inheritdoc />
    public async Task<PlaybackControlResult> ExecuteAsync(
        IDiscordMessage message,
        string arguments,
        CancellationToken token = default)
    {
        var guildId = message.Channel.Guild.Id;
        try
        {
            LavalinkTrack? seed = sessions.CaptureCurrent(guildId)?.Player.CurrentTrack;

            if (!string.IsNullOrWhiteSpace(arguments))
            {
                var loaded = await audio.Tracks.LoadTracksAsync(arguments, resolver.ResolveSearchMode(arguments), cancellationToken: token);
                seed = loaded.IsPlaylist ? null : loaded.Track;
            }

            if (seed is null)
            {
                return Fail(guildId, LocalizationKeys.DiscoveryNoTrack);
            }

            var choices = await GetRecommendationsAsync(guildId, seed, token);

            if (choices.Count == 0)
            {
                return Fail(guildId, LocalizationKeys.RecommendationsUnavailable);
            }

            var text = string.Join("\n", choices.Select((track, index) =>
                $"{index + 1}. {Bound(track.Author)} - {Bound(track.Title)}\n{track.Uri}"));
            return PlaybackControlResult.Succeeded(localization.Get(guildId, LocalizationKeys.RecommendationsList, text));
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Recommendation lookup failed. Guild={GuildId}", guildId);
            return Fail(guildId, LocalizationKeys.RecommendationsUnavailable);
        }
    }

    /// <inheritdoc />
    public async Task<PlaybackControlResult> AutoplayAsync(
        IDiscordMessage message,
        string arguments,
        CancellationToken token = default)
    {
        var guildId = message.Channel.Guild.Id;
        var value = arguments.Trim().ToLowerInvariant();

        if (value is "" or "status")
        {
            var enabled = await settings.GetAutoplayAsync(guildId, token);
            return PlaybackControlResult.Succeeded(localization.Get(guildId,
                enabled ? LocalizationKeys.AutoplayEnabled : LocalizationKeys.AutoplayDisabled));
        }

        if (value is not ("on" or "off"))
        {
            return Fail(guildId, LocalizationKeys.DiscoveryInvalid);
        }

        var member = await message.Channel.Guild.GetMemberAsync(message.Author.Id)
            .WaitAsync(token);
        var session = sessions.CaptureCurrent(guildId);

        if (session is null
            || member.VoiceState?.Channel?.Id != session.Player.VoiceChannelId)
        {
            return Fail(guildId, LocalizationKeys.PlaybackWrongVoiceChannel);
        }

        var reservation = sessions.ReserveControl(session, "AutoplaySetting");
        try
        {
            sessions.ValidateControl(session, reservation);
            await settings.SetAutoplayAsync(guildId, value == "on", token);
            ManualRequest(guildId);
            return PlaybackControlResult.Succeeded(localization.Get(guildId,
                value == "on" ? LocalizationKeys.AutoplayEnabled : LocalizationKeys.AutoplayDisabled));
        }
        finally
        {
            sessions.ReleaseControl(reservation);
        }
    }

    /// <inheritdoc />
    public async Task<bool> TryEnqueueAsync(PlaybackSession session, QueueItemRecord seed, CancellationToken token = default)
    {
        var state = _states.GetOrAdd(session.GuildId, _ => new());
        long revision;
        lock (state.Sync)
        {
            if (state.AutomaticCount >= 10)
            {
                return false;
            }

            revision = state.Revision;
        }

        if (!await settings.GetAutoplayAsync(session.GuildId, token))
        {
            return false;
        }

        try
        {
            var original = serializer.Deserialize(seed.TrackIdentifier).ToLavalinkTrack();
            var choices = await GetRecommendationsAsync(session.GuildId, original, token);

            if (choices.Count == 0)
            {
                return false;
            }

            var track = choices[0];
            return await sessions.MutateAsync(session, async () =>
            {
                lock (state.Sync)
                {
                    if (state.Revision != revision || state.AutomaticCount >= 10)
                    {
                        return false;
                    }
                }

                if (!await settings.GetAutoplayAsync(session.GuildId, token))
                {
                    return false;
                }

                var item = new QueueItemToEnqueue(serializer.Serialize(new LavaLinkTrackWrapper(track)),
                    track.Uri?.ToString(), nameof(TrackSearchMode.None), AutomaticRequester);
                lock (state.Sync)
                {
                    if (state.Revision != revision || state.AutomaticCount >= 10)
                    {
                        return false;
                    }

                    // Reserve the slot before a commit can become uncertain.
                    state.AutomaticCount++;
                }

                var enqueued = await queue.EnqueueAutoplayIfEmptyAsync(session.GuildId, item, token);

                if (!enqueued)
                {
                    lock (state.Sync)
                    {
                        if (state.Revision == revision)
                        {
                            state.AutomaticCount--;
                        }
                    }
                }

                return enqueued;
            });
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Autoplay unavailable; no remote playback retry. Guild={GuildId}", session.GuildId);
            return false;
        }
    }

    /// <summary>Loads real source recommendations and excludes the seed, recent history and queued occurrences.</summary>
    private async Task<IReadOnlyList<LavalinkTrack>> GetRecommendationsAsync(
        ulong guildId,
        LavalinkTrack seed,
        CancellationToken token)
    {
        string? radio = null;

        if (string.Equals(seed.SourceName, "spotify", StringComparison.OrdinalIgnoreCase))
        {
            radio = "sprec:mix:track:" + seed.Identifier;
        }
        else
        {
            var youtube = seed;

            if (!string.Equals(seed.SourceName, "youtube", StringComparison.OrdinalIgnoreCase))
            {
                var search = await audio.Tracks.LoadTracksAsync(seed.Author + " " + seed.Title,
                    TrackSearchMode.YouTube, cancellationToken: token);
                youtube = search.Track;
            }

            if (youtube is not null
                && string.Equals(youtube.SourceName, "youtube", StringComparison.OrdinalIgnoreCase))
            {
                radio = "https://www.youtube.com/watch?v=" + Uri.EscapeDataString(youtube.Identifier)
                    + "&list=RD" + Uri.EscapeDataString(youtube.Identifier);
            }
        }

        if (radio is null)
        {
            return [];
        }

        var loaded = await audio.Tracks.LoadTracksAsync(radio, TrackSearchMode.None, cancellationToken: token);
        var excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { Key(seed), Name(seed) };
        var state = _states.GetOrAdd(guildId, _ => new());
        lock (state.Sync)
        {
            foreach (var recent in state.Recent)
            {
                excluded.Add(recent.Source);
                excluded.Add(recent.Name);
            }
        }

        var waiting = await queue.GetQueuedItemsAsync(guildId, token);

        foreach (var item in waiting)
        {
            try
            {
                var track = serializer.Deserialize(item.TrackIdentifier).ToLavalinkTrack();
                excluded.Add(Key(track));
                excluded.Add(Name(track));
            }
            catch (Exception exception)
            {
                logger.LogDebug(exception, "Could not inspect queued recommendation identity. QueueItemId={QueueItemId}", item.Id);
            }
        }

        var result = new List<LavalinkTrack>();

        foreach (var track in loaded.Tracks)
        {
            if (track.IsLiveStream
                || excluded.Contains(Key(track))
                || excluded.Contains(Name(track)))
            {
                continue;
            }

            excluded.Add(Key(track));
            excluded.Add(Name(track));
            result.Add(track);

            if (result.Count == 5)
            {
                break;
            }
        }

        return result;
    }

    /// <summary>Builds a source-specific stable identity.</summary>
    private static string Key(LavalinkTrack track) => track.SourceName + ":" + track.Identifier;

    /// <summary>Builds a display identity to suppress mirrored copies of the same recording.</summary>
    private static string Name(LavalinkTrack track) => "name:" + track.Author.Trim() + ":" + track.Title.Trim();

    /// <summary>Bounds provider metadata in a five-result response.</summary>
    private static string Bound(string value) => value.Length > 70 ? value[..67] + "..." : value;

    /// <summary>Creates a localized validation or availability result.</summary>
    private PlaybackControlResult Fail(ulong guildId, string key) =>
        PlaybackControlResult.Failed(localization.Get(guildId, key), key);
}
