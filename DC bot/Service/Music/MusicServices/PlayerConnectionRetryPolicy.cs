using DC_bot.Helper.Validation;
using DC_bot.Interface.Core;
using Lavalink4NET.Players;

namespace DC_bot.Service.Music.MusicServices;

internal sealed class PlayerConnectionRetryPolicy(IValidationService validationService)
{
    private const int MaxAttempts = 5;
    private const int DelayMs = 200;

    internal async Task<ConnectionValidationResult> ValidateAsync(
        ILavalinkPlayer connection,
        CancellationToken cancellationToken)
    {
        ConnectionValidationResult validationConnectionResult = null!;
        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            validationConnectionResult = await validationService.ValidateConnectionAsync(connection).ConfigureAwait(false);
            if (validationConnectionResult.IsValid)
            {
                break;
            }

            await Task.Delay(DelayMs, cancellationToken).ConfigureAwait(false);
        }

        return validationConnectionResult;
    }
}
