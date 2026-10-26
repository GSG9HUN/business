using Lavalink4NET.Players;

namespace DC_bot.Helper.Validation;

/// <summary>
/// Carries outcome and response data for connection validation operations.
/// </summary>
/// <param name="isValid">The is valid supplied for this operation.</param>
/// <param name="errorKey">The error key supplied for this operation.</param>
/// <param name="connection">The lavalink player? dependency used by this boundary.</param>
public class ConnectionValidationResult(bool isValid, string errorKey, ILavalinkPlayer? connection)
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
    /// The validated Lavalink connection, or null when no connection is available.
    /// </summary>
    public ILavalinkPlayer? Connection { get; } = connection;
}
