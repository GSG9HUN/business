using System.Diagnostics;
using DC_bot.Exceptions.Music;
using DC_bot.Helper.Validation;
using DC_bot.Interface.Core;
using Lavalink4NET;
using Lavalink4NET.Players;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.Lifecycle;

internal sealed class PlayerConnectionRetryPolicy(
    IValidationService validationService,
    ILogger<PlayerConnectionService> logger)
{
    private const int DelayMs = 500;

    internal async Task<ConnectionValidationResult> ValidateJoinedPlayerAsync(
        IAudioService audioService,
        ulong guildId,
        ILavalinkPlayer? joinedConnection,
        CancellationToken cancellationToken)
    {
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

                var registered = await audioService.Players
                    .GetPlayerAsync(guildId, cancellationToken)
                    .ConfigureAwait(false);

                if (!ReferenceEquals(registered, joinedConnection))
                {
                    throw new PlaybackSessionExpiredException();
                }

                var validation = await validationService
                    .ValidateConnectionAsync(joinedConnection)
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
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            logger.LogError(
                "Join readiness wait cancelled or deadline reached. " +
                "Guild={GuildId} Attempts={Attempts} ElapsedMs={ElapsedMs} " +
                "ConnectionState={ConnectionState} IsConnected={IsConnected} " +
                "PlayerState={PlayerState} VoiceChannelId={VoiceChannelId}",
                guildId,
                attempts,
                elapsed.ElapsedMilliseconds,
                joinedConnection.ConnectionState,
                joinedConnection.ConnectionState.IsConnected,
                joinedConnection.State,
                joinedConnection.VoiceChannelId);

            throw;
        }
    }
}