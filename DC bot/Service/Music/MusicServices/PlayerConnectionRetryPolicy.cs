using DC_bot.Helper.Validation;
using DC_bot.Interface.Core;
using Lavalink4NET;
using Lavalink4NET.Players;

namespace DC_bot.Service.Music.MusicServices;

internal sealed class PlayerConnectionRetryPolicy(IValidationService validationService)
{
    private const int MaxAttempts = 5;
    private const int DelayMs = 200;

    internal async Task<ConnectionValidationResult> ValidateJoinedPlayerAsync(
        IAudioService audioService,
        ulong guildId,
        ILavalinkPlayer? joinedConnection,
        CancellationToken cancellationToken)
    {
        ConnectionValidationResult validationConnectionResult =
            new(false, Constants.ValidationErrorKeys.BotIsNotConnectedError, null);
        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            if (joinedConnection is not null)
            {
                validationConnectionResult = await validationService
                    .ValidateConnectionAsync(joinedConnection)
                    .ConfigureAwait(false);
                if (validationConnectionResult.IsValid)
                {
                    return validationConnectionResult;
                }
            }

            var playerValidationResult = await validationService
                .ValidatePlayerAsync(audioService, guildId)
                .ConfigureAwait(false);
            if (playerValidationResult is { IsValid: true, Player: not null })
            {
                validationConnectionResult = await validationService
                    .ValidateConnectionAsync(playerValidationResult.Player)
                    .ConfigureAwait(false);
                if (validationConnectionResult.IsValid)
                {
                    return validationConnectionResult;
                }
            }
            else if (joinedConnection is null && !string.IsNullOrWhiteSpace(playerValidationResult.ErrorKey))
            {
                validationConnectionResult = new ConnectionValidationResult(
                    false,
                    playerValidationResult.ErrorKey,
                    null);
            }

            await Task.Delay(DelayMs, cancellationToken).ConfigureAwait(false);
        }

        return validationConnectionResult;
    }
}
