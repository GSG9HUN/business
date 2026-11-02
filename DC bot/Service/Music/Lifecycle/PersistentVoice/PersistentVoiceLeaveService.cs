using DC_bot.Configuration;
using DC_bot.Interface.Service.Persistence.Guilds;
using DC_bot.Interface.Service.Persistence.Queue;
using DC_bot.Service.Music.Lifecycle.Cleanup;
using DC_bot.Service.Music.Lifecycle.Models;
using DC_bot.Service.Music.Lifecycle.Sessions;
using Microsoft.Extensions.Options;

namespace DC_bot.Service.Music.Lifecycle.PersistentVoice;

/// <summary>
/// Disables persistent voice before admitting explicit leave cleanup.
/// </summary>
/// <param name="guildRepository">Persists the user's disabled voice preference.</param>
/// <param name="queueRepository">Closes the active batch after successful cleanup.</param>
/// <param name="sessions">Owns generation invalidation and cleanup.</param>
/// <param name="cleanupService">Settles local, remote, and persisted player state.</param>
/// <param name="persistentVoiceGate">Serializes leave against recovery and settings changes.</param>
/// <param name="options">Provides independent cleanup deadlines.</param>
public sealed class PersistentVoiceLeaveService(
    IGuildDataRepository guildRepository,
    IQueueRepository queueRepository,
    GuildPlaybackSessions sessions,
    PlaybackCleanupService cleanupService,
    PersistentVoiceGate persistentVoiceGate,
    IOptions<PlaybackLifecycleOptions> options)
{

    /// <summary>
    /// Executes admitted leave independently of the command caller's cancellation.
    /// </summary>
    /// <param name="guildId">The guild whose persistent voice preference and playback are stopped.</param>
    /// <returns>The phase results after cleanup and active batch closure have completed.</returns>
    /// <remarks>
    /// Once admitted, this operation owns its lifetime. Callers may cancel their wait, but must observe late failures.
    /// A failed preference write prevents disconnection so that recovery cannot undo an explicit leave.
    /// </remarks>
    public async Task<LeaveResult> LeaveAsync(ulong guildId)
    {
        var gate = persistentVoiceGate.Get(guildId);
        await gate.WaitAsync();

        try
        {
            using (var persistenceDeadline = new CancellationTokenSource(options.Value.LeaveDeadline))
            {
                await guildRepository.SetTwentyFourSevenSettingsAsync(
                    guildId,
                    false,
                    null,
                    null,
                    persistenceDeadline.Token);
            }

            var result = await sessions.LeaveAsync(
                guildId,
                player => cleanupService.CleanupAsync(guildId, player, joinWasNeverInvoked: false));

            if (result.FullySucceeded)
            {
                using var persistenceDeadline = new CancellationTokenSource(options.Value.LeaveDeadline);
                await queueRepository.CloseActiveBatchAsync(guildId, cancellationToken: persistenceDeadline.Token);
            }

            return result;
        }
        finally
        {
            gate.Release();
        }
    }
}
