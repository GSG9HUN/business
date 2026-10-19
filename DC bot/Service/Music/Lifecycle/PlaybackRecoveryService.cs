using DC_bot.BotControl;
using DC_bot.Interface.Service.Music.Playback.Models;
using DC_bot.Interface.Service.Persistence.Playback;
using DC_bot.Interface.Service.Persistence.Queue;
using Lavalink4NET;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DC_bot.Service.Music.Lifecycle;

public sealed class PlaybackRecoveryService(
    GuildPlaybackSessions sessions,
    PlaybackRemoteProbe probe,
    IAudioService audioService,
    IQueueRepository queueRepository,
    IOptions<PlaybackLifecycleOptions> options,
    ILogger<PlaybackRecoveryService> logger)
{
    public async Task<bool> TryConfirmStartAsync(
        PlaybackSession session, ControlReservation reservation,
        Guid operationId, long queueItemId,
        string nodeSessionId, string encodedTrack, string? previousEncodedTrack)
    {
        // Same-track replacement cannot be disambiguated by a snapshot.
        if (encodedTrack == previousEncodedTrack || session.Player.SessionId != nodeSessionId || !sessions.IsAttemptCurrent(session, reservation, operationId, queueItemId, encodedTrack))
        {
            return false;
        }

        using var deadline = new CancellationTokenSource(options.Value.LeaveDeadline);
        try
        {
            var matches = await probe.IsPlayingAsync(nodeSessionId, session.GuildId, encodedTrack, deadline.Token);
           
            return matches && session.Player.SessionId == nodeSessionId &&
                sessions.IsAttemptCurrent(session, reservation, operationId, queueItemId, encodedTrack);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Remote start remains uncertain. Guild={GuildId} OperationId={OperationId}",
                session.GuildId, operationId);
            return false;
        }
    }

    public async Task<bool> CommitKnownStartAsync(PlaybackSession session, long queueItemId, Guid operationId, string serializedTrack, PlaybackStartReason reason)
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
                        if (!await queueRepository.ConfirmStartTransportAsync(session.GuildId, queueItemId, operationId, deadline.Token))
                        {
                            return false;
                        }
                        
                        return await queueRepository.CompleteStartAsync(
                            session.GuildId, queueItemId, operationId, serializedTrack,
                            reason == PlaybackStartReason.Previous
                                ? MobileRealtimeEventNames.PlaybackPreviousStarted
                                : MobileRealtimeEventNames.PlaybackStarted,
                            clearSourceMetadata: reason == PlaybackStartReason.Reload,
                            cancellationToken: deadline.Token);
                    });
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex,
                        "Known start commit pending. Guild={GuildId} OperationId={OperationId} Attempt={Attempt}",
                        session.GuildId, operationId, attempt + 1);
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

    public async Task<bool> RecoverBeforeJoinAsync(ulong guildId, CancellationToken cancellationToken)
    {
        var pending = await queueRepository.GetPendingStartsAsync(guildId, cancellationToken);
        if (pending.Count == 0)
        {
            return true;
        }

        return await sessions.RecoverWithoutPlayerAsync(guildId, async () =>
        {
            // Re-read after entering the lifecycle boundary.
            if (await audioService.Players.GetPlayerAsync(guildId, cancellationToken) is not null)
            {
                return false;
            }

            var current = await queueRepository.GetPendingStartsAsync(guildId, cancellationToken);
            foreach (var item in current)
            {
                if (item.OperationId == Guid.Empty || string.IsNullOrWhiteSpace(item.NodeAddress) || string.IsNullOrWhiteSpace(item.NodeSessionId) || !await probe.IsSessionGoneAsync(item.NodeAddress, item.NodeSessionId, cancellationToken))
                {
                    logger.LogWarning(
                        "Pending start requires remote evidence. Guild={GuildId} QueueItemId={QueueItemId} OperationId={OperationId}",
                        guildId, item.QueueItemId, item.OperationId);
                    return false;
                }

                if (!await queueRepository.ResolveDisconnectedStartAsync(guildId, item.QueueItemId, item.OperationId, cancellationToken))
                {
                    return false;
                }
            }
            
            return true;
        });
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await RecoverPendingAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
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
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
        }
    }

    public async Task RecoverPendingAsync(CancellationToken cancellationToken)
    {
        var pending = await queueRepository.GetPendingStartsAsync(
            cancellationToken: cancellationToken);
        foreach (var guildId in pending.Select(item => item.GuildId).Distinct())
        {
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            deadline.CancelAfter(options.Value.LeaveDeadline);
            try
            {
                await RecoverBeforeJoinAsync(guildId, deadline.Token);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
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