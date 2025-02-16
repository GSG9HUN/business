using DC_bot.Interface;

namespace DC_bot.Helper;

public class ValidationResult(bool isValid, IDiscordMember? member = null)
{
    public bool IsValid { get; } = isValid;
    public IDiscordMember? Member { get; } = member;
}