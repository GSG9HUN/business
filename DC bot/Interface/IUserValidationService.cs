using DC_bot.Helper;

namespace DC_bot.Interface;

public interface IUserValidationService
{
    public Task<UserValidationResult> ValidateUserAsync(IDiscordMessage message);
    public bool IsBotUser(IDiscordMessage message);
}