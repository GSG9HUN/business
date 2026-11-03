using DC_bot.Exceptions.Music;
using DC_bot.Service.Music.Lifecycle.Models;
using DC_bot.Service.Music.Lifecycle.Sessions;
using Lavalink4NET;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.Lifecycle.Cleanup;

/// <summary>
/// Reconciles locally owned players that are no longer registered with Lavalink4NET.
/// </summary>
/// <param name="audioService">Provides the registered Lavalink players.</param>
/// <param name="sessions">Coordinates player generations and fenced cleanup.</param>
/// <param name="playbackCleanupService">Settles remote and persisted playback state.</param>
/// <param name="logger">Records cleanup outcomes and unresolved lifecycle state.</param>
public sealed class DisconnectedPlayerCleanupService(
    IAudioService audioService,
    GuildPlaybackSessions sessions,
    PlaybackCleanupService playbackCleanupService,
    ILogger<DisconnectedPlayerCleanupService> logger)
{

    /// <summary>
    /// Cleans up an orphaned local session before another join is attempted.
    /// </summary>
    /// <remarks>
    /// Missing registration does not prove that remote playback has stopped.
    /// The existing cleanup procedure must confirm and settle that state.
    /// Caller cancellation stops waiting but does not cancel admitted cleanup.
    /// </remarks>
    /// <param name="guildId">The Discord guild preparing to join voice.</param>
    /// <param name="cancellationToken">Cancels player lookup and waiting for cleanup.</param>
    /// <returns>A task that completes when cleanup is unnecessary or fully successful.</returns>
    public async Task ReconcileBeforeJoinAsync(
        ulong guildId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var captured = sessions.CaptureCurrent(guildId);

        if (captured is null)
        {
            return;
        }

        var registered = await audioService.Players.GetPlayerAsync(
            guildId,
            cancellationToken);

        if (registered is not null)
        {
            return;
        }

        logger.LogWarning(
            "An owned player is no longer registered. "
            + "Reconciling before join. Guild={GuildId} Generation={Generation}",
            guildId,
            captured.Generation);

        Task<LeaveResult> cleanupTask;

        try
        {
            cleanupTask = sessions.LeaveIfCurrentAsync(
                captured,
                player => playbackCleanupService.CleanupAsync(
                    guildId,
                    player,
                    joinWasNeverInvoked: false));
        }
        catch (PlaybackSessionExpiredException)
        {
            // A concurrent operation replaced or invalidated the captured session.
            // Its lifecycle owner is responsible for the resulting state.
            return;
        }

        // Observe completion even if the join caller stops waiting.
        _ = ObserveCleanupAsync(cleanupTask, guildId);

        var result = await cleanupTask.WaitAsync(cancellationToken);

        if (!result.FullySucceeded)
        {
            throw new PlaybackOperationBusyException(
                "The disconnected player cleanup has not fully settled.");
        }
    }

    /// <summary>
    /// Observes admitted cleanup independently of the join caller's cancellation.
    /// </summary>
    /// <param name="cleanupTask">The cleanup task owned by the lifecycle coordinator.</param>
    /// <param name="guildId">The Discord guild being cleaned up.</param>
    /// <returns>A task that completes after the cleanup outcome has been recorded.</returns>
    private async Task ObserveCleanupAsync(Task<LeaveResult> cleanupTask, ulong guildId)
    {
        try
        {
            var result = await cleanupTask.ConfigureAwait(false);

            if (!result.FullySucceeded)
            {
                logger.LogWarning(
                    "Disconnected player cleanup remains pending. "
                    + "Guild={GuildId} Result={@Result}",
                    guildId,
                    result);

                return;
            }

            logger.LogInformation(
                "Disconnected player cleanup completed. Guild={GuildId}",
                guildId);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Disconnected player cleanup failed. Guild={GuildId}",
                guildId);
        }
    }
}