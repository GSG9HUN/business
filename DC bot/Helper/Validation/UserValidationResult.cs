using DC_bot.Interface.Discord;

namespace DC_bot.Helper.Validation;

public class UserValidationResult(bool isValid, string errorKey, IDiscordMember? member = null)
{
    public bool IsValid { get; } = isValid;
    public string ErrorKey { get; } = errorKey;
    public IDiscordMember? Member { get; } = member;
}