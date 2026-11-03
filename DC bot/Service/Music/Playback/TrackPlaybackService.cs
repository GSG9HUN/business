using DC_bot.Constants;
using DC_bot.Exceptions.Music;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.Playback;
using DC_bot.Interface.Service.Music.Playback.Models;
using DC_bot.Interface.Service.Music.Presentation;
using DC_bot.Interface.Service.Music.Queue;
using DC_bot.Interface.Service.Music.Queue.Models;
using DC_bot.Interface.Service.Music.Tracks;
using DC_bot.Interface.Service.Persistence.Models.Queue;
using DC_bot.Interface.Service.Persistence.Queue;
using DC_bot.Service.Music.Lifecycle;
using DC_bot.Service.Music.Lifecycle.Models;
using DC_bot.Service.Music.Lifecycle.Recovery;
using DC_bot.Service.Music.Lifecycle.Sessions;
using DC_bot.Wrapper.Lavalink;
using Lavalink4NET.Players;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.Playback;

/// <summary>
/// Claims queue items, dispatches playback, and commits or reconciles results under session ownership.
/// </summary>
/// <param name="musicQueueService">The music queue service dependency used by this boundary.</param>
/// <param name="trackNotificationService">The track notification service dependency used by this boundary.</param>
/// <param name="localizationService">The localization service dependency used by this boundary.</param>
/// <param name="queueRepository">The queue repository dependency used by this boundary.</param>
/// <param name="trackSerializer">The track serializer dependency used by this boundary.</param>
/// <param name="playbackRecoveryService">The playback recovery service supplied for this operation.</param>
/// <param name="playbackRemoteProbe">The playback remote probe supplied for this operation.</param>
/// <param name="sessions">The sessions supplied for this operation.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
public sealed class TrackPlaybackService(
    IMusicQueueService musicQueueService,
    ITrackNotificationService trackNotificationService,
    ILocalizationService localizationService,
    IQueueRepository queueRepository,
    ITrackSerializer trackSerializer,
    PlaybackRecoveryService playbackRecoveryService,
    PlaybackRemoteProbe playbackRemoteProbe,
    GuildPlaybackSessions sessions,
    ILogger<TrackPlaybackService> logger) : ITrackPlaybackService
{

    /// <summary>
    /// Adds loaded tracks to the active batch and starts playback when the player has no current track.
    /// </summary>
    /// <param name="searchQuery">The search query supplied for this operation.</param>
    /// <param name="session">The captured player generation whose ownership must remain current.</param>
    /// <param name="textChannel">The Discord channel used by the operation.</param>
    /// <param name="requestedBy">The requested by supplied for this operation.</param>
    /// <param name="sourceQuery">The source query supplied for this operation.</param>
    /// <param name="sourceSearchMode">The source search mode supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public async Task<PlaybackStartResult> PlayTheFoundMusicAsync(
        TrackLoadResult searchQuery,
        PlaybackSession session,
        IDiscordChannel textChannel,
        string? requestedBy,
        string? sourceQuery,
        TrackSearchMode? sourceSearchMode,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var operationId = Guid.NewGuid();

        if (!sessions.IsCurrent(session))
        {
            return new(PlaybackStartStatus.Superseded, operationId);
        }

        var tracks = searchQuery.IsPlaylist ? searchQuery.Tracks.ToList() : searchQuery.Track is { } single ? [single] : [];

        if (tracks.Count == 0 || searchQuery.IsFailed)
        {
            return new(PlaybackStartStatus.NotStarted, operationId);
        }

        try
        {
            await sessions.MutateAsync(session, async () =>
            {
                await musicQueueService.EnqueueMany(session.GuildId, tracks.Select(track => new QueueTrackToEnqueue(new LavaLinkTrackWrapper(track), searchQuery.IsPlaylist ? track.Uri?.ToString() : sourceQuery, sourceSearchMode, requestedBy))
                    .ToList(), cancellationToken);
                return true;
            });
        }
        catch (PlaybackSessionExpiredException)
        {
            return new(PlaybackStartStatus.Superseded, operationId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Playback enqueue uncertain. OperationId={OperationId} Guild={GuildId} Phase=Enqueue Category=Persistence", operationId, session.GuildId);
            return new(PlaybackStartStatus.Unknown, operationId);
        }

        if (session.Player.CurrentTrack is null)
        {
            return await TryPlayNextTrackAsync(session, textChannel, PlaybackStartReason.First, default, cancellationToken: cancellationToken);
        }

        try
        {
            var text = tracks.Count > 1 ? localizationService.Get(session.GuildId, LocalizationKeys.PlayCommandListAddedQueue) : $"{localizationService.Get(session.GuildId, LocalizationKeys.PlayCommandMusicAddedQueue)} {tracks[0].Author} - {tracks[0].Title}";
            await trackNotificationService.SendSafeAsync(textChannel, text, "PlaybackQueued", cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Queue notification failed. OperationId={OperationId} Guild={GuildId} Phase=Notify Category=Notification", operationId, session.GuildId);
        }

        return new(PlaybackStartStatus.Queued, operationId);
    }

    /// <summary>
    /// Claims and starts the next waiting track without blindly retrying uncertain remote dispatch.
    /// </summary>
    /// <param name="session">The captured player generation whose ownership must remain current.</param>
    /// <param name="textChannel">The Discord channel used by the operation.</param>
    /// <param name="reason">The reason supplied for this operation.</param>
    /// <param name="reservation">The caller&apos;s exclusive control reservation, when one is supplied.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public Task<PlaybackStartResult> TryPlayNextTrackAsync(
        PlaybackSession session,
        IDiscordChannel textChannel,
        PlaybackStartReason reason = PlaybackStartReason.QueueNext,
        ControlReservation? reservation = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return StartAsync(session, textChannel, null, reason, null, reservation, null, cancellationToken);
    }

    /// <summary>
    /// Starts the specified durable queue item under the supplied or newly acquired reservation.
    /// </summary>
    /// <param name="session">The captured player generation whose ownership must remain current.</param>
    /// <param name="textChannel">The Discord channel used by the operation.</param>
    /// <param name="queueItemId">The durable queue-item identity used for playback and persistence correlation.</param>
    /// <param name="reason">The reason supplied for this operation.</param>
    /// <param name="replacement">The lava link track? dependency used by this boundary.</param>
    /// <param name="resume">The optional confirmed recovery position and pause state; null starts normally.</param>
    /// <param name="reservation">The caller&apos;s exclusive control reservation, when one is supplied.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public Task<PlaybackStartResult> StartExistingAsync(
        PlaybackSession session,
        IDiscordChannel textChannel,
        long queueItemId,
        PlaybackStartReason reason,
        ILavaLinkTrack? replacement = null,
        ControlReservation? reservation = null,
        PlaybackResumeOptions? resume = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return StartAsync(session, textChannel, queueItemId, reason, replacement, reservation, resume, cancellationToken);
    }

    /// <summary>
    /// Acquires or validates playback ownership before starting the selected queue item.
    /// </summary>
    /// <param name="session">The captured player generation whose ownership must remain current.</param>
    /// <param name="channel">The Discord channel used by the operation.</param>
    /// <param name="requestedQueueItemId">The durable queue-item identity used for playback and persistence correlation.</param>
    /// <param name="reason">The reason supplied for this operation.</param>
    /// <param name="replacement">The lava link track? dependency used by this boundary.</param>
    /// <param name="resume">The optional recovery checkpoint forwarded unchanged to dispatch and commit.</param>
    /// <param name="reservation">The caller&apos;s exclusive control reservation, when one is supplied.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    private async Task<PlaybackStartResult> StartAsync(
        PlaybackSession session,
        IDiscordChannel channel,
        long? requestedQueueItemId,
        PlaybackStartReason reason,
        ILavaLinkTrack? replacement,
        ControlReservation? reservation,
        PlaybackResumeOptions? resume,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (resume is not null
            && (resume.Position < TimeSpan.Zero
                || resume.Position.TotalSeconds > int.MaxValue))
        {
            throw new ArgumentOutOfRangeException(nameof(resume), "Recovery position is outside supported storage bounds.");
        }

        var operationId = Guid.NewGuid();
        ControlReservation? ownedReservation = null;
        try
        {
            if (reservation is null)
            {
                ownedReservation = sessions.ReserveControl(session, $"Start:{reason}");
                reservation = ownedReservation;
            }
            else
            {
                sessions.ValidateControl(session, reservation);
            }

            return await StartCoreAsync(session, channel, requestedQueueItemId, reason, replacement, reservation, operationId, resume, cancellationToken);
        }
        catch (PlaybackSessionExpiredException)
        {
            return new(PlaybackStartStatus.Superseded, operationId);
        }
        catch (PlaybackOperationBusyException)
        {
            return new(PlaybackStartStatus.Busy, operationId);
        }
        finally
        {
            // The caller releases an inherited reservation.
            if (ownedReservation is not null)
            {
                sessions.ReleaseControl(ownedReservation);
            }
        }
    }

    /// <summary>
    /// Claims, binds, dispatches, and commits one attempt while tracking whether remote dispatch may have occurred.
    /// </summary>
    /// <remarks>Reconcile uncertain dispatch instead of blindly replaying or requeueing it, to avoid starting the same queue item twice.</remarks>
    /// <param name="session">The captured player generation whose ownership must remain current.</param>
    /// <param name="channel">The Discord channel used by the operation.</param>
    /// <param name="requestedQueueItemId">The durable queue-item identity used for playback and persistence correlation.</param>
    /// <param name="reason">The reason supplied for this operation.</param>
    /// <param name="replacement">The lava link track? dependency used by this boundary.</param>
    /// <param name="resume">The recovery checkpoint applied to the remote start and committed playback state.</param>
    /// <param name="reservation">The caller&apos;s exclusive control reservation, when one is supplied.</param>
    /// <param name="operationId">The correlation identifier of the playback or command operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    private async Task<PlaybackStartResult> StartCoreAsync(
        PlaybackSession session,
        IDiscordChannel channel,
        long? requestedQueueItemId,
        PlaybackStartReason reason,
        ILavaLinkTrack? replacement,
        ControlReservation reservation,
        Guid operationId,
        PlaybackResumeOptions? resume,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        QueueStartClaim? claim = null;
        var dispatched = false;
        var started = false;
        try
        {
            sessions.BeginAttempt(session, operationId, reservation, replace: reason == PlaybackStartReason.Previous);
            var selection = await sessions.MutateAsync(session, () => queueRepository.ClaimForStartAsync(session.GuildId, operationId, requestedQueueItemId, cancellationToken: cancellationToken));

            if (selection.Status != QueueClaimStatus.Claimed)
            {
                sessions.AbandonAttempt(session, operationId);
                return new(selection.Status == QueueClaimStatus.Empty ? PlaybackStartStatus.Empty : PlaybackStartStatus.Busy, operationId);
            }

            claim = selection.Claim ?? throw new InvalidOperationException("Claimed result has no claim.");
            ILavaLinkTrack track;
            try
            {
                track = replacement ?? trackSerializer.Deserialize(claim.Item.TrackIdentifier, claim.Item.Id);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Invalid queued track. Guild={GuildId} OperationId={OperationId}", session.GuildId, operationId);
                await queueRepository.ReleaseStartAsync(session.GuildId, claim.Item.Id, operationId, skip: true);
                sessions.AbandonAttempt(session, operationId);
                return new(PlaybackStartStatus.NotStarted, operationId, claim.Item.Id);
            }

            var serializedTrack = trackSerializer.Serialize(track);
            await sessions.RunTransportAsync(session, async () =>
            {
                var playableTrack = await track.ToLavalinkTrack()
                    .GetPlayableTrackAsync(cancellationToken: cancellationToken);
                var encodedTrack = playableTrack.ToString();
                var nodeSessionId = session.Player.SessionId;
                var previousEncodedTrack = session.Player.CurrentTrack?.ToString();

                if (string.IsNullOrWhiteSpace(nodeSessionId))
                {
                    throw new InvalidOperationException("Player has no node session.");
                }

                // Persist remote scope BEFORE any PlayAsync may leave this process.
                // Network I/O does not run inside the repository transaction.
                var bound = await sessions.MutateAsync(session, () => queueRepository.BindStartNodeAsync(session.GuildId, claim.Item.Id, operationId, playbackRemoteProbe.NodeAddress, nodeSessionId, cancellationToken: cancellationToken));

                if (!bound)
                {
                    throw new InvalidOperationException("Start claim ownership changed.");
                }

                sessions.BindAttempt(session, operationId, claim.Item.Id, encodedTrack);

                if (session.Player.SessionId != nodeSessionId)
                {
                    throw new InvalidOperationException("Node session changed before dispatch.");
                }

                dispatched = true;
                try
                {
                    if (resume is { IsPaused: true })
                    {
                        await playbackRemoteProbe.StartPausedAsync(
                            nodeSessionId,
                            session.GuildId,
                            encodedTrack,
                            resume.Position,
                            cancellationToken);

                        await session.Player.RefreshAsync(cancellationToken);
                    }
                    else
                    {
                        var properties = new TrackPlayProperties
                        {
                            StartPosition = resume?.Position
                        };

                        await session.Player.PlayAsync(
                            playableTrack,
                            properties,
                            cancellationToken);
                    }
                    started = true;
                }
                catch (Exception transportError)
                {
                    // Read-only, bounded confirmation. No second PlayAsync.
                    var confirmed = await playbackRecoveryService.TryConfirmStartAsync(session, reservation, operationId, claim.Item.Id, nodeSessionId, encodedTrack, previousEncodedTrack);

                    if (!confirmed)
                    {
                        throw;
                    }

                    started = true;
                    logger.LogWarning(transportError, "Remote start confirmed after transport error. Guild={GuildId} OperationId={OperationId}", session.GuildId, operationId);
                }

                return true;
            });
            var committed = await playbackRecoveryService.CommitKnownStartAsync(
                session,
                claim.Item.Id,
                operationId,
                serializedTrack,
                reason,
                resume?.Position,
                resume?.IsPaused ?? false);

            if (!committed)
            {
                return new(PlaybackStartStatus.StartedStatePending, operationId, claim.Item.Id);
            }

            if (sessions.IsCurrent(session))
            {
                try
                {
                    await trackNotificationService.NotifyNowPlayingAsync(channel, track, resume?.Position ?? track.StartPosition ?? TimeSpan.Zero, track.Duration, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Playback notification failed. Guild={GuildId} OperationId={OperationId}", session.GuildId, operationId);
                }
            }

            return new(PlaybackStartStatus.Started, operationId, claim.Item.Id);
        }
        catch (PlaybackOperationBusyException)
        {
            return new(PlaybackStartStatus.Busy, operationId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Playback start incomplete. Guild={GuildId} OperationId={OperationId} Dispatched={Dispatched}", session.GuildId, operationId, dispatched);

            if (dispatched)
            {
                // No AbandonAttempt or ReleaseStart for an uncertain remote start.
                return new(started ? PlaybackStartStatus.StartedStatePending : PlaybackStartStatus.Unknown, operationId, claim?.Item.Id);
            }

            if (claim is not null)
            {
                try
                {
                    await queueRepository.ReleaseStartAsync(session.GuildId, claim.Item.Id, operationId, skip: false);
                }
                catch (Exception releaseError)
                {
                    logger.LogError(releaseError, "Claim release uncertain. Guild={GuildId} OperationId={OperationId}", session.GuildId, operationId);
                    return new(PlaybackStartStatus.Unknown, operationId, claim.Item.Id);
                }
            }

            sessions.AbandonAttempt(session, operationId);
            return new(ex is PlaybackSessionExpiredException ? PlaybackStartStatus.Superseded : PlaybackStartStatus.NotStarted, operationId, claim?.Item.Id);
        }
    }
}
