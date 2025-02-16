using DC_bot.Helper;

namespace DC_bot.Interface;

public interface IUserValidationService
{
    public Task<ValidationResult> ValidateUserAsync(IDiscordMessage message);
    public bool IsBotUser(IDiscordMessage message);
}