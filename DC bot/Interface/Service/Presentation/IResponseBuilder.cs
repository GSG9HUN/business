using DC_bot.Interface.Discord;

namespace DC_bot.Interface.Service.Presentation;

public interface IResponseBuilder
{
    Task SendValidationErrorAsync(IDiscordMessage message, string errorKey);
    Task SendUsageAsync(IDiscordMessage message, string commandName);
    Task SendSuccessAsync(IDiscordMessage message, string localizationKey, params object[] args);
    Task SendWarningAsync(IDiscordMessage message, string localizationKey, params object[] args);
    Task SendErrorAsync(IDiscordMessage message, string localizationKey, params object[] args);
}
