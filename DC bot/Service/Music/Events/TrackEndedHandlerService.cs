using DC_bot.BotControl;
using DC_bot.Exceptions.Music;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music.Events;
using DC_bot.Interface.Service.Music.Playback;
using DC_bot.Interface.Service.Music.Playback.Models;
using DC_bot.Interface.Service.Music.Presentation;
using DC_bot.Interface.Service.Music.Repeat;
using DC_bot.Interface.Service.Persistence.Models.Queue;
using DC_bot.Interface.Service.Persistence.Playback;
using DC_bot.Interface.Service.Persistence.Queue;
using DC_bot.Service.Music.Lifecycle;
using DC_bot.Service.Music.Lifecycle.Models;
using DC_bot.Service.Music.Lifecycle.Sessions;
using DC_bot.Wrapper.Lavalink;
using Lavalink4NET;
using Lavalink4NET.Events.Players;
using Lavalink4NET.Protocol.Payloads.Events;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.Events;

/// <summary>
/// Completes the correlated attempt, then advances, reloads, repeats, or closes its batch.
/// </summary>
/// <param name="audioService">The audio service dependency used by this boundary.</param>
/// <param name="repeatService">The repeat service dependency used by this boundary.</param>
/// <param name="currentTrackService">The current track service dependency used by this boundary.</param>
/// <param name="trackPlaybackService">The track playback service dependency used by this boundary.</param>
/// <param name="trackNotificationService">The track notification service dependency used by this boundary.</param>
/// <param name="queueRepository">The queue repository dependency used by this boundary.</param>
/// <param name="playbackStateRepository">The playback state repository dependency used by this boundary.</param>
/// <param name="sessions">The sessions supplied for this operation.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
public sealed class TrackEndedHandlerService(
    IAudioService audioService,
    IRepeatService repeatService,
    ICurrentTrackService currentTrackService,
    ITrackPlaybackService trackPlaybackService,
    ITrackNotificationService trackNotificationService,
    IQueueRepository queueRepository,
    IPlaybackStateRepository playbackStateRepository,
    GuildPlaybackSessions sessions,
    ILogger<TrackEndedHandlerService> logger) : ITrackEndedHandlerService
{

    /// <summary>
    /// Completes the owned queue attempt, then reloads, repeats, advances, or closes the batch for its end reason.
    /// </summary>
    /// <param name="session">The captured player generation whose ownership must remain current.</param>
    /// <param name="operationId">The correlation identifier of the playback or command operation.</param>
    /// <param name="queueItemId">The durable queue-item identity used for playback and persistence correlation.</param>
    /// <param name="args">The event payload supplied by the Discord or Lavalink SDK.</param>
    /// <param name="textChannel">The Discord channel used by the operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task HandleTrackEndedAsync(
        PlaybackSession session,
        Guid operationId,
        long queueItemId,
        TrackEndedEventArgs args,
        IDiscordChannel textChannel)
    {
        if (!sessions.IsCurrent(session) || args.Player.GuildId != session.GuildId)
        {
            return;
        }

        ControlReservation? reservation = null;
        try
        {
            reservation = await sessions.ReserveControlAsync(session, "TrackEnded");
            var accepted = await sessions.MutateAsync(session, () => queueRepository.CompleteAttemptAsync(session.GuildId, queueItemId, operationId, played: args.Reason == TrackEndReason.Finished));

            if (!accepted || !ShouldContinuePlayback(args.Reason))
            {
                return;
            }

            var endedItem = await queueRepository.GetByIdAsync(queueItemId);

            if (endedItem is null)
            {
                return;
            }

            if (args.Reason == TrackEndReason.LoadFailed)
            {
                var reload = await TryReloadFailedTrackAsync(session, textChannel, endedItem, reservation);

                if (reload is not null)
                {
                    LogNonSuccess(reload, session.GuildId);
                    return;
                }
            }

            if (args.Reason == TrackEndReason.Finished
                && await repeatService.IsRepeatingAsync(session.GuildId, default))
            {
                var repeated = await trackPlaybackService.StartExistingAsync(session, textChannel, queueItemId, PlaybackStartReason.RepeatCurrent, reservation: reservation, cancellationToken: default);
                LogNonSuccess(repeated, session.GuildId);
                return;
            }

            var next = await trackPlaybackService.TryPlayNextTrackAsync(session, textChannel, reservation: reservation, cancellationToken: default);

            if (next.Status != PlaybackStartStatus.Empty)
            {
                LogNonSuccess(next, session.GuildId);
                return;
            }

            var list = await TryRepeatListAndPlayAsync(session, textChannel, reservation);

            if (list.Status != PlaybackStartStatus.Empty)
            {
                LogNonSuccess(list, session.GuildId);
                return;
            }

            await sessions.MutateAsync(session, async () =>
            {
                await queueRepository.CloseBatchIfIdleAsync(session.GuildId);
                await currentTrackService.SetCurrentTrackAsync(session.GuildId, null, MobileRealtimeEventNames.PlaybackStopped);
                return true;
            });

            if (sessions.IsCurrent(session))
            {
                await trackNotificationService.NotifyQueueEmptyAsync(textChannel, default);
            }
        }
        catch (PlaybackSessionExpiredException)
        {
            logger.LogDebug("Ignored superseded track-end. OperationId={OperationId} Guild={GuildId}", operationId, session.GuildId);
        }
        catch (PlaybackOperationBusyException ex)
        {
            // An unresolved attempt is a recovery case, not permission
            // to retry PlayAsync or requeue an uncertain start.
            logger.LogWarning(ex, "Track-end blocked by pending playback. OperationId={OperationId} Guild={GuildId}", operationId, session.GuildId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Track-end processing interrupted without auto-retry. OperationId={OperationId} Guild={GuildId} Phase=TrackEnd Category=Orchestration", operationId, session.GuildId);
        }
        finally
        {
            if (reservation is not null)
            {
                sessions.ReleaseControl(reservation);
            }
        }
    }

    /// <summary>
    /// Selects end reasons that permit queue advancement or source recovery.
    /// </summary>
    /// <param name="reason">The reason supplied for this operation.</param>
    /// <returns>Whether the requested check or operation succeeded.</returns>
    private static bool ShouldContinuePlayback(TrackEndReason reason) => reason is TrackEndReason.Finished or TrackEndReason.Stopped or TrackEndReason.LoadFailed;

    /// <summary>
    /// Reloads from saved source metadata before retrying the same durable queue item.
    /// </summary>
    /// <param name="session">The captured player generation whose ownership must remain current.</param>
    /// <param name="channel">The Discord channel used by the operation.</param>
    /// <param name="item">The item supplied for this operation.</param>
    /// <param name="reservation">The caller&apos;s exclusive control reservation, when one is supplied.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    private async Task<PlaybackStartResult?> TryReloadFailedTrackAsync(
        PlaybackSession session,
        IDiscordChannel channel,
        QueueItemRecord item,
        ControlReservation reservation)
    {
        if (string.IsNullOrWhiteSpace(item.SourceQuery)
            || !TryParseSearchMode(item.SourceSearchMode, out var mode))
        {
            await NotifyPlaybackLoadFailedAsync(session, item);
            return null;
        }

        TrackLoadResult loaded;
        try
        {
            loaded = await sessions.RunTransportAsync(session, async () => await audioService.Tracks.LoadTracksAsync(item.SourceQuery, mode));
        }
        catch (PlaybackSessionExpiredException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Reload failed. Guild={GuildId} QueueItemId={QueueItemId} Phase=Reload Category=Source", session.GuildId, item.Id);
            await NotifyPlaybackLoadFailedAsync(session, item);
            return null;
        }

        var track = loaded.Track ?? loaded.Tracks.FirstOrDefault();

        if (track is null || loaded.IsFailed)
        {
            await NotifyPlaybackLoadFailedAsync(session, item);
            return null;
        }

        return await trackPlaybackService.StartExistingAsync(session, channel, item.Id, PlaybackStartReason.Reload, new LavaLinkTrackWrapper(track) { QueueItemId = item.Id }, reservation: reservation, cancellationToken: default);
    }

    /// <summary>
    /// Publishes the failed item&apos;s playback-load state while the session remains current.
    /// </summary>
    /// <param name="session">The captured player generation whose ownership must remain current.</param>
    /// <param name="item">The item supplied for this operation.</param>
    /// <returns>A task yielding whether the requested check or operation succeeded.</returns>
    private Task<bool> NotifyPlaybackLoadFailedAsync(PlaybackSession session, QueueItemRecord item) => sessions.MutateAsync(session, async () =>
    {
        await playbackStateRepository.SetCurrentTrackAsync(session.GuildId, item.TrackIdentifier, item.Id, MobileRealtimeEventNames.PlaybackLoadFailed);
        return true;
    });

    /// <summary>
    /// Restarts canonical batch occurrences when enabled, then attempts to play the first occurrence.
    /// </summary>
    /// <remarks>Repeated queue occurrences reference canonical items. They must not become new canonical batch items, which would grow the list on every round.</remarks>
    /// <param name="session">The captured player generation whose ownership must remain current.</param>
    /// <param name="channel">The Discord channel used by the operation.</param>
    /// <param name="reservation">The caller&apos;s exclusive control reservation, when one is supplied.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    private async Task<PlaybackStartResult> TryRepeatListAndPlayAsync(
        PlaybackSession session,
        IDiscordChannel channel,
        ControlReservation reservation)
    {
        if (!await repeatService.IsRepeatingListAsync(session.GuildId, default))
        {
            return new(PlaybackStartStatus.Empty, Guid.NewGuid());
        }

        var restarted = await sessions.MutateAsync(session, () => queueRepository.RestartActiveBatchAsync(session.GuildId));

        if (!restarted)
        {
            return new(PlaybackStartStatus.Empty, Guid.NewGuid());
        }

        return await trackPlaybackService.TryPlayNextTrackAsync(session, channel, PlaybackStartReason.RepeatList, reservation: reservation, cancellationToken: default);
    }

    /// <summary>
    /// Accepts supported persisted provider names and rejects unknown search modes.
    /// </summary>
    /// <param name="value">The value supplied for this operation.</param>
    /// <param name="mode">The mode supplied for this operation.</param>
    /// <returns>Whether the requested check or operation succeeded.</returns>
    private static bool TryParseSearchMode(string? value, out TrackSearchMode mode)
    {
        switch (value?.Trim())
        {
            case nameof(TrackSearchMode.YouTube):
                mode = TrackSearchMode.YouTube;
                return true;
            case nameof(TrackSearchMode.YouTubeMusic):
                mode = TrackSearchMode.YouTubeMusic;
                return true;
            case nameof(TrackSearchMode.SoundCloud):
                mode = TrackSearchMode.SoundCloud;
                return true;
            case nameof(TrackSearchMode.Spotify):
                mode = TrackSearchMode.Spotify;
                return true;
            case nameof(TrackSearchMode.AppleMusic):
                mode = TrackSearchMode.AppleMusic;
                return true;
            case nameof(TrackSearchMode.Deezer):
                mode = TrackSearchMode.Deezer;
                return true;
            case nameof(TrackSearchMode.YandexMusic):
                mode = TrackSearchMode.YandexMusic;
                return true;
            case nameof(TrackSearchMode.Bandcamp):
                mode = TrackSearchMode.Bandcamp;
                return true;
            case nameof(TrackSearchMode.None):
                mode = TrackSearchMode.None;
                return true;
            default:
                mode = TrackSearchMode.None;
                return false;
        }
    }

    /// <summary>
    /// Records an automatic playback outcome that did not reach Started.
    /// </summary>
    /// <param name="result">The result supplied for this operation.</param>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    private void LogNonSuccess(PlaybackStartResult result, ulong guildId)
    {
        if (result.Status == PlaybackStartStatus.Started)
        {
            return;
        }

        logger.LogWarning("Automatic advance stopped. OperationId={OperationId} Guild={GuildId} Status={Status} QueueItemId={QueueItemId}", result.OperationId, guildId, result.Status, result.QueueItemId);
    }
}
