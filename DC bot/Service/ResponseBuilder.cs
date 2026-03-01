using DC_bot.Interface;

namespace DC_bot.Service;

public class ResponseBuilder(ILocalizationService localization) : IResponseBuilder
{
    public async Task SendValidationErrorAsync(IDiscordMessage message, string errorKey)
    {
        if (string.IsNullOrEmpty(errorKey))
        {
            return;
        }
        
        await message.RespondAsync(localization.Get(errorKey));
    }

    public async Task SendUsageAsync(IDiscordMessage message, string commandName)
    {
        await message.RespondAsync(localization.Get($"{commandName}_command_usage"));
    }

    public async Task SendSuccessAsync(IDiscordMessage message, string text)
    {
        await message.RespondAsync(text);
    }

    public async Task SendCommandResponseAsync(IDiscordMessage message, string commandName)
    {
        await message.RespondAsync(localization.Get($"{commandName}_command_response"));
    }

    public async Task SendCommandErrorResponse(IDiscordMessage message, string commandName)
    {
        await message.RespondAsync(localization.Get($"{commandName}_command_error"));
    }
}