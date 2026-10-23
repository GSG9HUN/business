using DC_bot.Interface.Discord;

namespace DC_bot.Helper.Validation;

/// <summary>
/// Carries outcome and response data for user validation operations.
/// </summary>
/// <param name="isValid">The is valid supplied for this operation.</param>
/// <param name="errorKey">The error key supplied for this operation.</param>
/// <param name="member">The discord member? dependency used by this boundary.</param>
public class UserValidationResult(bool isValid, string errorKey, IDiscordMember? member = null)
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
    /// The Discord guild member associated with this operation, or null when unavailable.
    /// </summary>
    public IDiscordMember? Member { get; } = member;
}
