using DC_bot.BotControl;
using DC_bot.Configuration;
using DC_bot.Interface.Service.Music.Events;
using DC_bot.Interface.Service.Persistence.GuildBotStatus;
using DC_bot.Interface.Service.Persistence.Guilds;
using DC_bot.Interface.Service.Persistence.Playback;
using DC_bot.Interface.Service.Persistence.Queue;
using DC_bot.Service.Music.Lifecycle.Models;
using DC_bot.Service.Music.Lifecycle.Recovery;
using Lavalink4NET;
using Lavalink4NET.Players;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DC_bot.Service.Music.Lifecycle.Cleanup;

/// <summary>
/// Runs bounded cleanup phases and reports local, remote, and persistence outcomes separately.
/// </summary>
/// <remarks>Cleanup owns independent phase deadlines. Caller cancellation must not abandon an uncertain remote player or incorrectly persist a confirmed disconnect. FullySucceeded requires local invalidation and the applicable timer, subscription, disconnect, and persistence phases to settle successfully; Stop is reported separately.</remarks>
/// <param name="audioService">The audio service dependency used by this boundary.</param>
/// <param name="eventHandlers">The playback event handler service dependency used by this boundary.</param>
/// <param name="probe">The probe supplied for this operation.</param>
/// <param name="queueRepository">The queue repository dependency used by this boundary.</param>
/// <param name="guildDataRepository">Determines whether a confirmed disconnect retains the recovery checkpoint.</param>
/// <param name="playbackStateRepository">The playback state repository dependency used by this boundary.</param>
/// <param name="guildBotStatusRepository">The guild bot status repository dependency used by this boundary.</param>
/// <param name="options">The configured typed options consumed by this boundary.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
public sealed class PlaybackCleanupService(
    IAudioService audioService,
    IPlaybackEventHandlerService eventHandlers,
    PlaybackRemoteProbe probe,
    IQueueRepository queueRepository,
    IGuildDataRepository guildDataRepository,
    IPlaybackStateRepository playbackStateRepository,
    IGuildBotStatusRepository guildBotStatusRepository,
    IOptions<PlaybackLifecycleOptions> options,
    ILogger<PlaybackCleanupService> logger)
{
    // Only invoke as a GuildPlaybackSessions.LeaveAsync cleanup callback.
    // LeaveAsync has already invalidated and drained the old generation.

    /// <summary>
    /// Runs cleanup phases and returns their separate outcomes instead of treating partial cleanup as success.
    /// </summary>
    /// <remarks>
    /// Invoke through leave after invalidating ownership and draining transports. Unknown remote effects remain fenced.
    /// Enabled persistent voice retains current track, position, and pause state; explicit leave disables it first.
    /// </remarks>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="player">The lavalink player? dependency used by this boundary.</param>
    /// <param name="joinWasNeverInvoked">Whether the SDK join was never called, proving this attempt created no remote player.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public async Task<LeaveResult> CleanupAsync(ulong guildId, ILavalinkPlayer? player, bool joinWasNeverInvoked)
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
            remote = await PhaseAsync(guildId, "Disconnect", async ct =>
            {
                logger.LogWarning("Bot requests voice disconnect during cleanup. " + "Guild={GuildId} NodeSessionId={NodeSessionId}", guildId, player.SessionId);
                await player.DisconnectAsync(ct);
            });

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
        if (
            remote is
            not (LeavePhaseState.Succeeded or LeavePhaseState.NotApplicable))
        {
            await PhaseAsync(guildId, "RemoteSessionEvidence", async ct =>
            {
                if (!string.IsNullOrWhiteSpace(disconnectedSessionId)
                    && await probe.IsPlayerGoneAsync(
                        disconnectedSessionId,
                        guildId,
                        ct))
                {
                    remote = LeavePhaseState.Succeeded;
                    return;
                }
                var pending = await queueRepository.GetPendingStartsAsync(guildId, ct);

                if (pending.Count == 0)
                {
                    return;
                }

                foreach (var item in pending)
                {
                    if (string.IsNullOrWhiteSpace(item.NodeAddress)
                        || string.IsNullOrWhiteSpace(item.NodeSessionId)
                        || !await probe.IsSessionGoneAsync(item.NodeAddress, item.NodeSessionId, ct))
                    {
                        return;
                    }
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
                    var matchingConfirmedDisconnect = item.TransportConfirmed && player is not null && item.NodeAddress == probe.NodeAddress && item.NodeSessionId == disconnectedSessionId && player.SessionId == disconnectedSessionId;

                    if (!matchingConfirmedDisconnect)
                    {
                        if (string.IsNullOrWhiteSpace(item.NodeAddress)
                            || string.IsNullOrWhiteSpace(item.NodeSessionId)
                            || !await probe.IsSessionGoneAsync(item.NodeAddress, item.NodeSessionId, ct))
                        {
                            throw new InvalidOperationException("Uncertain remote transport is not fenced.");
                        }
                    }

                    if (
                        !await queueRepository.ResolveDisconnectedStartAsync(
                        guildId,
                        item.QueueItemId,
                        item.OperationId,
                        ct))
                    {
                        throw new InvalidOperationException("Pending ownership changed during cleanup.");
                    }
                }

                var settings = await guildDataRepository.GetTwentyFourSevenSettingsAsync(
                    guildId,
                    ct);

                if (!settings.IsEnabled)
                {
                    await playbackStateRepository.SetCurrentTrackAsync(
                        guildId,
                        null,
                        null,
                        MobileRealtimeEventNames.PlaybackStopped,
                        ct);
                }
            });
        }

        var result = new LeaveResult(LeavePhaseState.Succeeded, LeavePhaseState.NotApplicable, subscription, stop, remote, voice, current);
        logger.LogInformation("Playback cleanup finished. Guild={GuildId} Result={@Result}", guildId, result);
        return result;
    }

    /// <summary>
    /// Runs one cleanup phase with its own deadline and translates its outcome into a phase state.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="phase">The phase supplied for this operation.</param>
    /// <param name="action">The cancellation request for waiting and supported downstream operations.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
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
