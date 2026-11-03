using System.Diagnostics;
using DC_bot.Exceptions.Music;
using DC_bot.Helper.Validation;
using DC_bot.Interface.Core;
using Lavalink4NET;
using Lavalink4NET.Players;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.Lifecycle.Connections;

/// <summary>
/// Waits for joined-player readiness while rejecting replaced players and honoring the join deadline.
/// </summary>
/// <param name="validationService">The validation service dependency used by this boundary.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
public sealed class PlayerConnectionRetryPolicy(IValidationService validationService, ILogger<PlayerConnectionService> logger)
{
    private const int DelayMs = 500;

    /// <summary>
    /// Polls readiness until validation succeeds, the player is replaced, or cancellation is requested.
    /// </summary>
    /// <param name="audioService">The audio service dependency used by this boundary.</param>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="joinedConnection">The lavalink player? dependency used by this boundary.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    internal async Task<ConnectionValidationResult> ValidateJoinedPlayerAsync(
        IAudioService audioService,
        ulong guildId,
        ILavalinkPlayer? joinedConnection,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (joinedConnection is null)
        {
            throw new InvalidOperationException("Join returned no player.");
        }

        var elapsed = Stopwatch.StartNew();
        var attempts = 0;
        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                attempts++;
                var registered = await audioService.Players.GetPlayerAsync(guildId, cancellationToken)
                    .ConfigureAwait(false);

                if (!ReferenceEquals(registered, joinedConnection))
                {
                    throw new PlaybackSessionExpiredException();
                }

                var validation = await validationService.ValidateConnectionAsync(joinedConnection, cancellationToken)
                    .WaitAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (validation.IsValid)
                {
                    return validation;
                }

                await Task.Delay(DelayMs, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
        {
            logger.LogError("Join readiness wait cancelled or deadline reached. " + "Guild={GuildId} Attempts={Attempts} ElapsedMs={ElapsedMs} " + "ConnectionState={ConnectionState} IsConnected={IsConnected} " + "PlayerState={PlayerState} VoiceChannelId={VoiceChannelId}", guildId, attempts, elapsed.ElapsedMilliseconds, joinedConnection.ConnectionState, joinedConnection.ConnectionState.IsConnected, joinedConnection.State, joinedConnection.VoiceChannelId);
            throw;
        }
    }
}
