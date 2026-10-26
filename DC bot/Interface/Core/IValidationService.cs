using DC_bot.Helper.Validation;
using Lavalink4NET.Players;

namespace DC_bot.Interface.Core;

/// <summary>
/// Defines the validation service boundary so callers are independent of its concrete implementation.
/// </summary>
public interface IValidationService
{

    /// <summary>
    /// Validates Lavalink readiness using available player and remote voice-state evidence.
    /// </summary>
    /// <param name="connection">The lavalink player dependency used by this boundary.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public Task<ConnectionValidationResult> ValidateConnectionAsync(
        ILavalinkPlayer connection,
        CancellationToken cancellationToken = default);
}
