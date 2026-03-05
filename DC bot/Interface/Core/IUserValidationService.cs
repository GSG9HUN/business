using DC_bot.Helper.Validation;
using DC_bot.Interface.Discord;

namespace DC_bot.Interface.Core;

public interface IUserValidationService
{
    public Task<UserValidationResult> ValidateUserAsync(IDiscordMessage message);
    public bool IsBotUser(IDiscordMessage message);
}