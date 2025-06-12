using DC_bot.Interface;

namespace DC_bot.Helper;

public class UserValidationResult(bool isValid, string errorKey, IDiscordMember? member = null)
{
    public bool IsValid { get; } = isValid;
    public string ErrorKey { get; } = errorKey;
    public IDiscordMember? Member { get; } = member;
}