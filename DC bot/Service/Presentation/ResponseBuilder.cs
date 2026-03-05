using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DC_bot.Service.Presentation;

public class ResponseBuilder(ILocalizationService localization, ILogger<ResponseBuilder>? logger = null) : IResponseBuilder
{
    private readonly ILogger<ResponseBuilder> _logger = logger ?? NullLogger<ResponseBuilder>.Instance;

    public async Task SendValidationErrorAsync(IDiscordMessage message, string errorKey)
    {
        if (string.IsNullOrEmpty(errorKey))
        {
            return;
        }

        await SafeRespondAsync(message, localization.Get(errorKey), "SendValidationErrorAsync");
    }

    public async Task SendUsageAsync(IDiscordMessage message, string commandName)
    {
        await SafeRespondAsync(message, localization.Get($"{commandName}_command_usage"), "SendUsageAsync");
    }

    public async Task SendSuccessAsync(IDiscordMessage message, string text)
    {
        await SafeRespondAsync(message, text, "SendSuccessAsync");
    }

    public async Task SendCommandResponseAsync(IDiscordMessage message, string commandName)
    {
        await SafeRespondAsync(message, localization.Get($"{commandName}_command_response"), "SendCommandResponseAsync");
    }

    public async Task SendCommandErrorResponse(IDiscordMessage message, string commandName)
    {
        await SafeRespondAsync(message, localization.Get($"{commandName}_command_error"), "SendCommandErrorResponse");
    }

    private async Task SafeRespondAsync(IDiscordMessage message, string text, string operation)
    {
        try
        {
            await message.RespondAsync(text);
        }
        catch (Exception ex)
        {
            _logger.ResponseSendFailed(ex, operation);
        }
    }
}