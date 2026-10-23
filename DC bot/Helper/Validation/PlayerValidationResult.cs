using Lavalink4NET.Players;

namespace DC_bot.Helper.Validation;

/// <summary>
/// Carries outcome and response data for player validation operations.
/// </summary>
/// <param name="isValid">The is valid supplied for this operation.</param>
/// <param name="errorKey">The error key supplied for this operation.</param>
/// <param name="player">The lavalink player? dependency used by this boundary.</param>
public class PlayerValidationResult(bool isValid, string errorKey, ILavalinkPlayer? player)
{

    /// <summary>
    /// Whether validation succeeded.
    /// </summary>
    public bool IsValid { get; } = isValid;

    /// <summary>
    /// The localization key explaining a failed validation.
    /// </summary>
    public string ErrorKey { get; } = errorKey;

    /// <summary>
    /// The validated Lavalink player, or null when no player is available.
    /// </summary>
    public ILavalinkPlayer? Player { get; } = player;
}
