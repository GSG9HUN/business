using DC_bot.Exceptions.Music;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music.Events;
using DC_bot.Service.Music.Lifecycle;
using DC_bot.Service.Music.Lifecycle.Sessions;
using Lavalink4NET;
using Lavalink4NET.Events;
using Lavalink4NET.Events.Players;
using Lavalink4NET.Players;
using Lavalink4NET.Protocol.Payloads.Events;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.Events;

/// <summary>
/// Correlates SDK end events to current attempts and retires previous player subscriptions safely.
/// </summary>
/// <param name="audioService">The audio service dependency used by this boundary.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
/// <param name="trackEndedHandlerService">The track ended handler service dependency used by this boundary.</param>
/// <param name="sessions">The sessions supplied for this operation.</param>
public sealed class PlaybackEventHandlerService(
    IAudioService audioService,
    ILogger<PlaybackEventHandlerService> logger,
    ITrackEndedHandlerService trackEndedHandlerService,
    GuildPlaybackSessions sessions) : IPlaybackEventHandlerService
{

    /// <summary>
    /// Keeps a player&apos;s track-end handler and enabled state together during subscription retirement.
    /// </summary>
    /// <param name="player">The lavalink player dependency used by this boundary.</param>
    /// <param name="handler">The handler supplied for this operation.</param>
    private sealed class Registration(ILavalinkPlayer player, AsyncEventHandler<TrackEndedEventArgs> handler)
    {
        public ILavalinkPlayer Player { get; } = player;
        public AsyncEventHandler<TrackEndedEventArgs> Handler { get; } = handler;
        public bool Enabled { get; set; } = true;
    }

    private readonly object _handlerLock = new();
    private readonly Dictionary<ulong, Registration> _trackEndedHandlers = new();
    private readonly Dictionary<ulong, List<Registration>> _retired = new();

    /// <summary>
    /// Disables and removes current and retired track-end subscriptions for the guild.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public Task CleanupGuildAsync(ulong guildId)
    {
        List<Exception> failures = new();
        lock (_handlerLock)
        {
            if (!_retired.TryGetValue(guildId, out var retired))
            {
                retired = new List<Registration>();
                _retired[guildId] = retired;
            }

            if (_trackEndedHandlers.Remove(guildId, out var active))
            {
                active.Enabled = false;
                retired.Add(active);
            }

            for (var index = retired.Count - 1; index >= 0; index--)
            {
                retired[index].Enabled = false;
                try
                {
                    audioService.TrackEnded -= retired[index].Handler;
                    retired.RemoveAt(index);
                }
                catch (Exception ex)
                {
                    failures.Add(ex);
                }
            }

            if (retired.Count == 0)
            {
                _retired.Remove(guildId);
            }
        }

        return failures.Count == 0 ? Task.CompletedTask : Task.FromException(new AggregateException("Playback unsubscribe failed.", failures));
    }

    /// <summary>
    /// Subscribes the current player to correlated end events while retiring previous subscriptions.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="connection">The lavalink player dependency used by this boundary.</param>
    /// <param name="textChannel">The Discord channel used by the operation.</param>
    public void RegisterPlaybackFinishedHandler(ulong guildId, ILavalinkPlayer connection, IDiscordChannel textChannel)
    {
        var session = sessions.Capture(guildId, connection) ?? throw new PlaybackSessionExpiredException();
        lock (_handlerLock)
        {
            if (_trackEndedHandlers.TryGetValue(guildId, out var existing))
            {
                if (existing.Enabled && ReferenceEquals(existing.Player, connection))
                {
                    return;
                }

                existing.Enabled = false;
                _trackEndedHandlers.Remove(guildId);

                if (!_retired.TryGetValue(guildId, out var retired))
                {
                    _retired[guildId] = retired = new List<Registration>();
                }

                retired.Add(existing);
                try
                {
                    audioService.TrackEnded -= existing.Handler;
                    retired.Remove(existing);

                    if (retired.Count == 0)
                    {
                        _retired.Remove(guildId);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Retired subscription remains pending cleanup. Guild: {GuildId}", guildId);
                }
            }

            Registration? registration = null;
            AsyncEventHandler<TrackEndedEventArgs> handler = (_, args) =>
            {
                lock (_handlerLock)
                {
                    if (registration is null
                        || !registration.Enabled
                        || !_trackEndedHandlers.TryGetValue(guildId, out var current)
                        || !ReferenceEquals(current, registration)
                        || !ReferenceEquals(args.Player, connection))
                    {
                        return Task.CompletedTask;
                    }
                }

                // Skip dispatches its own end after StopAsync succeeds. A delayed
                // Stopped/Replaced/Cleanup event must not consume the next attempt.
                if (
                    args.Reason is
                    not (TrackEndReason.Finished or TrackEndReason.LoadFailed))
                {
                    return Task.CompletedTask;
                }

                var accepted = sessions.OnTrackEnded(session, args.Track.ToString(), async (operationId, queueItemId) =>
                {
                    try
                    {
                        if (!sessions.IsCurrent(session))
                        {
                            return;
                        }

                        await trackEndedHandlerService.HandleTrackEndedAsync(session, operationId, queueItemId, args, textChannel);
                    }
                    catch (PlaybackSessionExpiredException)
                    {
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Track-end processing failed. OperationId: {OperationId}, Guild: {GuildId}, Phase: {Phase}, Category: {Category}", operationId, guildId, "TrackEnded", "Orchestration");
                    }
                });
                return Task.CompletedTask;
            };
            registration = new Registration(connection, handler);
            _trackEndedHandlers[guildId] = registration;
            try
            {
                audioService.TrackEnded += handler;
            }
            catch
            {
                registration.Enabled = false;
                _trackEndedHandlers.Remove(guildId);

                if (!_retired.TryGetValue(guildId, out var retired))
                {
                    _retired[guildId] = retired = new List<Registration>();
                }

                retired.Add(registration);
                throw;
            }
        }
    }
}
