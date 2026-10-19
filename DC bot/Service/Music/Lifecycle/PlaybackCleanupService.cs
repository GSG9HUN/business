using DC_bot.BotControl;
using DC_bot.Interface.Service.Music.Events;
using DC_bot.Interface.Service.Persistence.GuildBotStatus;
using DC_bot.Interface.Service.Persistence.Playback;
using DC_bot.Interface.Service.Persistence.Queue;
using Lavalink4NET;
using Lavalink4NET.Players;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DC_bot.Service.Music.Lifecycle;

public sealed class PlaybackCleanupService(
    IAudioService audioService,
    IPlaybackEventHandlerService eventHandlers,
    PlaybackRemoteProbe probe,
    IQueueRepository queueRepository,
    IPlaybackStateRepository playbackStateRepository,
    IGuildBotStatusRepository guildBotStatusRepository,
    IOptions<PlaybackLifecycleOptions> options,
    ILogger<PlaybackCleanupService> logger)
{
    // Only invoke as a GuildPlaybackSessions.LeaveAsync cleanup callback.
    // LeaveAsync has already invalidated and drained the old generation.
    public async Task<LeaveResult> CleanupAsync(
        ulong guildId, ILavalinkPlayer? player, bool joinWasNeverInvoked)
    {
        var subscription = await PhaseAsync(guildId, "Subscription", async _ => await eventHandlers.CleanupGuildAsync(guildId));
        var stop = LeavePhaseState.NotApplicable;
        var remote = LeavePhaseState.Unknown;

        if (player is null)
        {
            await PhaseAsync(guildId, "ResolvePlayer", async ct => player = await audioService.Players.GetPlayerAsync(guildId, ct));
        }

        var disconnectedSessionId = player?.SessionId;
        if (player is not null)
        {
            stop = await PhaseAsync(guildId, "Stop", async ct => await player.StopAsync(ct));
            // Stop failure must not skip the disconnect attempt.
            remote = await PhaseAsync(guildId, "Disconnect", async ct => await player.DisconnectAsync(ct));
           
            if (player.SessionId != disconnectedSessionId)
            {
                remote = LeavePhaseState.Unknown;
            }
        }
        else if (joinWasNeverInvoked)
        {
            // Evidence is SDK join was never called, not registry absence.
            remote = LeavePhaseState.NotApplicable;
        }

        // A missing managed player does not prove absence. A dead, durably
        // identified old node session can provide that proof instead.
        if (remote is not (LeavePhaseState.Succeeded or LeavePhaseState.NotApplicable))
        {
            await PhaseAsync(guildId, "RemoteSessionEvidence", async ct =>
            {
                if (!string.IsNullOrWhiteSpace(disconnectedSessionId) && await probe.IsSessionGoneAsync(probe.NodeAddress, disconnectedSessionId, ct))
                {
                    remote = LeavePhaseState.Succeeded;
                    return;
                }

                var pending = await queueRepository.GetPendingStartsAsync(guildId, ct);
                if (pending.Count == 0) return;
               
                foreach (var item in pending)
                {
                    if (string.IsNullOrWhiteSpace(item.NodeAddress) ||
                        string.IsNullOrWhiteSpace(item.NodeSessionId) ||
                        !await probe.IsSessionGoneAsync(item.NodeAddress, item.NodeSessionId, ct))
                        return;
                }
                remote = LeavePhaseState.Succeeded;
            });
        }

        var voice = LeavePhaseState.NotAttempted;
        var current = LeavePhaseState.NotAttempted;
        
        if (remote is LeavePhaseState.Succeeded or LeavePhaseState.NotApplicable)
        {
            voice = await PhaseAsync(guildId, "VoicePersistence", ct => guildBotStatusRepository.MarkDisconnectedVoiceAsync(guildId, ct));

            current = await PhaseAsync(guildId, "CurrentAndPendingPersistence", async ct =>
            {
                var pending = await queueRepository.GetPendingStartsAsync(guildId, ct);
                if (pending.Count != 0 && remote != LeavePhaseState.Succeeded)
                {
                    throw new InvalidOperationException("Pending playback needs remote evidence.");
                }

                foreach (var item in pending)
                {
                    var matchingConfirmedDisconnect =
                        item.TransportConfirmed && player is not null &&
                        item.NodeAddress == probe.NodeAddress &&
                        item.NodeSessionId == disconnectedSessionId &&
                        player.SessionId == disconnectedSessionId;
                    if (!matchingConfirmedDisconnect)
                    {
                        if (string.IsNullOrWhiteSpace(item.NodeAddress) ||
                            string.IsNullOrWhiteSpace(item.NodeSessionId) ||
                            !await probe.IsSessionGoneAsync(item.NodeAddress, item.NodeSessionId, ct))
                            throw new InvalidOperationException("Uncertain remote transport is not fenced.");
                    }
                    
                    if (!await queueRepository.ResolveDisconnectedStartAsync(guildId, item.QueueItemId, item.OperationId, ct))
                    {
                        throw new InvalidOperationException("Pending ownership changed during cleanup.");
                    }
                }
                await playbackStateRepository.SetCurrentTrackAsync(guildId, null, null, MobileRealtimeEventNames.PlaybackStopped, ct);
            });
        }

        var result = new LeaveResult(LeavePhaseState.Succeeded, LeavePhaseState.NotApplicable, subscription, stop, remote, voice, current);
        logger.LogInformation("Playback cleanup finished. Guild={GuildId} Result={@Result}", guildId, result);
        return result;
    }

    private async Task<LeavePhaseState> PhaseAsync(ulong guildId, string phase, Func<CancellationToken, Task> action)
    {
        // Fresh cleanup deadline; never reuse the cancelled join token.
        using var deadline = new CancellationTokenSource(options.Value.LeaveDeadline);
        try
        {
            await action(deadline.Token);
            return LeavePhaseState.Succeeded;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Playback cleanup phase failed. Guild={GuildId} Phase={Phase}", guildId, phase);
            return LeavePhaseState.Failed;
        }
    }
}