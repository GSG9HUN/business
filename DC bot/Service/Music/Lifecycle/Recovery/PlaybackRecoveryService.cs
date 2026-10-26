using DC_bot.BotControl;
using DC_bot.Configuration;
using DC_bot.Interface.Service.Music.Playback.Models;
using DC_bot.Interface.Service.Persistence.Queue;
using DC_bot.Service.Music.Lifecycle.Models;
using DC_bot.Service.Music.Lifecycle.Sessions;
using Lavalink4NET;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DC_bot.Service.Music.Lifecycle.Recovery;

/// <summary>
/// Reconciles durable pending starts with remote player evidence before another attempt may begin.
/// </summary>
/// <param name="sessions">The sessions supplied for this operation.</param>
/// <param name="probe">The probe supplied for this operation.</param>
/// <param name="audioService">The audio service dependency used by this boundary.</param>
/// <param name="queueRepository">The queue repository dependency used by this boundary.</param>
/// <param name="options">The configured typed options consumed by this boundary.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
public sealed class PlaybackRecoveryService(
    GuildPlaybackSessions sessions,
    PlaybackRemoteProbe probe,
    IAudioService audioService,
    IQueueRepository queueRepository,
    IOptions<PlaybackLifecycleOptions> options,
    ILogger<PlaybackRecoveryService> logger)
{

    /// <summary>
    /// Checks remote player evidence to determine whether a durable pending start succeeded.
    /// </summary>
    /// <param name="session">The captured player generation whose ownership must remain current.</param>
    /// <param name="reservation">The caller&apos;s exclusive control reservation, when one is supplied.</param>
    /// <param name="operationId">The correlation identifier of the playback or command operation.</param>
    /// <param name="queueItemId">The durable queue-item identity used for playback and persistence correlation.</param>
    /// <param name="nodeSessionId">The Lavalink node-session identity associated with the player.</param>
    /// <param name="encodedTrack">The encoded track supplied for this operation.</param>
    /// <param name="previousEncodedTrack">The previous encoded track supplied for this operation.</param>
    /// <returns>A task yielding whether the requested check or operation succeeded.</returns>
    public async Task<bool> TryConfirmStartAsync(
        PlaybackSession session,
        ControlReservation reservation,
        Guid operationId,
        long queueItemId,
        string nodeSessionId,
        string encodedTrack,
        string? previousEncodedTrack)
    {
        // Same-track replacement cannot be disambiguated by a snapshot.
        if (encodedTrack == previousEncodedTrack
            || session.Player.SessionId != nodeSessionId
            || !sessions.IsAttemptCurrent(session, reservation, operationId, queueItemId, encodedTrack))
        {
            return false;
        }

        using var deadline = new CancellationTokenSource(options.Value.LeaveDeadline);
        try
        {
            var matches = await probe.IsPlayingAsync(nodeSessionId, session.GuildId, encodedTrack, deadline.Token);
            return matches && session.Player.SessionId == nodeSessionId && sessions.IsAttemptCurrent(session, reservation, operationId, queueItemId, encodedTrack);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Remote start remains uncertain. Guild={GuildId} OperationId={OperationId}", session.GuildId, operationId);
            return false;
        }
    }

    /// <summary>
    /// Commits an observed remote start to queue and current-track persistence under the session fence.
    /// </summary>
    /// <param name="session">The captured player generation whose ownership must remain current.</param>
    /// <param name="queueItemId">The durable queue-item identity used for playback and persistence correlation.</param>
    /// <param name="operationId">The correlation identifier of the playback or command operation.</param>
    /// <param name="serializedTrack">The serialized track supplied for this operation.</param>
    /// <param name="reason">The reason supplied for this operation.</param>
    /// <param name="initialPosition">The recovery position to commit atomically with the confirmed start, or null for zero.</param>
    /// <param name="initiallyPaused">Whether the confirmed remote start was restored in a paused state.</param>
    /// <returns>A task yielding whether the requested check or operation succeeded.</returns>
    public async Task<bool> CommitKnownStartAsync(
        PlaybackSession session,
        long queueItemId,
        Guid operationId,
        string serializedTrack,
        PlaybackStartReason reason,
        TimeSpan? initialPosition = null,
        bool initiallyPaused = false)
    {
        using var deadline = new CancellationTokenSource(options.Value.LeaveDeadline);
        try
        {
            // Idempotent persistence retry only. Never retry PlayAsync here.
            for (var attempt = 0; attempt < 2; attempt++)
            {
                try
                {
                    return await sessions.MutateAsync(session, async () =>
                    {
                        if (
                            !await queueRepository.ConfirmStartTransportAsync(
                            session.GuildId,
                            queueItemId,
                            operationId,
                            deadline.Token))
                        {
                            return false;
                        }

                        return await queueRepository.CompleteStartAsync(session.GuildId, queueItemId, operationId, serializedTrack, reason == PlaybackStartReason.Previous ? MobileRealtimeEventNames.PlaybackPreviousStarted : MobileRealtimeEventNames.PlaybackStarted, clearSourceMetadata: reason == PlaybackStartReason.Reload, initialPosition: initialPosition, initiallyPaused: initiallyPaused, cancellationToken: deadline.Token);
                    });
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Known start commit pending. Guild={GuildId} OperationId={OperationId} Attempt={Attempt}", session.GuildId, operationId, attempt + 1);

                    if (deadline.IsCancellationRequested || !sessions.IsCurrent(session))
                    {
                        return false;
                    }
                }
            }

            return false;
        }
        finally
        {
            // Remote start is proven on both callers of this method.
            // Release a queued early end event even if persistence is pending.
            sessions.CompleteAttempt(session, operationId);
        }
    }

    /// <summary>
    /// Reconciles stale local and remote playback state before a new join is permitted.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task yielding whether the requested check or operation succeeded.</returns>
    public async Task<bool> RecoverBeforeJoinAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var pending = await queueRepository.GetPendingStartsAsync(guildId, cancellationToken);

        if (pending.Count == 0)
        {
            return true;
        }

        return await sessions.RecoverWithoutPlayerAsync(guildId, async () =>
        {
            // Re-read after entering the lifecycle boundary.
            if (
                await audioService.Players.GetPlayerAsync(
                guildId,
                cancellationToken)is not null)
            {
                return false;
            }

            var current = await queueRepository.GetPendingStartsAsync(guildId, cancellationToken);

            foreach (var item in current)
            {
                if (item.OperationId == Guid.Empty
                    || string.IsNullOrWhiteSpace(item.NodeAddress)
                    || string.IsNullOrWhiteSpace(item.NodeSessionId)
                    || !await probe.IsSessionGoneAsync(item.NodeAddress, item.NodeSessionId, cancellationToken))
                {
                    logger.LogWarning("Pending start requires remote evidence. Guild={GuildId} QueueItemId={QueueItemId} OperationId={OperationId}", guildId, item.QueueItemId, item.OperationId);
                    return false;
                }

                if (
                    !await queueRepository.ResolveDisconnectedStartAsync(
                    guildId,
                    item.QueueItemId,
                    item.OperationId,
                    cancellationToken))
                {
                    return false;
                }
            }

            return true;
        });
    }

    /// <summary>
    /// Periodically reconciles durable pending playback starts until application shutdown, logging failed scans without ending the loop.
    /// </summary>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await RecoverPendingAsync(cancellationToken);
            }
            catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Pending playback recovery scan failed.");
            }

            try
            {
                // Polling interval is scheduling, never evidence to release a claim.
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            }
            catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
        }
    }

    /// <summary>
    /// Reconciles unresolved durable starts against the current player and remote node state.
    /// </summary>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task RecoverPendingAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var pending = await queueRepository.GetPendingStartsAsync(cancellationToken: cancellationToken);

        foreach (var guildId in pending.Select(item => item.GuildId)
            .Distinct())
        {
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            deadline.CancelAfter(options.Value.LeaveDeadline);
            try
            {
                await RecoverBeforeJoinAsync(guildId, deadline.Token);
            }
            catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Guild recovery remains pending. Guild={GuildId}", guildId);
            }
        }
    }
}
