using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DC_bot.Service.Presentation;

public class ResponseBuilder(ILocalizationService localization, ILogger<ResponseBuilder>? logger = null)
    : IResponseBuilder
{
    private const string ErrorPrefixKey = "response_error_prefix";
    private const string WarningPrefixKey = "response_warning_prefix";

    private readonly ILogger<ResponseBuilder> _logger = logger ?? NullLogger<ResponseBuilder>.Instance;

    public async Task SendValidationErrorAsync(IDiscordMessage message, string errorKey)
    {
        if (string.IsNullOrEmpty(errorKey)) return;

        await SafeRespondAsync(message, GetForMessage(message, errorKey), "SendValidationErrorAsync");
    }

    public async Task SendUsageAsync(IDiscordMessage message, string commandName)
    {
        await SafeRespondAsync(message, GetForMessage(message, $"{commandName}_command_usage"), "SendUsageAsync");
    }

    public async Task SendSuccessAsync(IDiscordMessage message, string localizationKey, params object[] args)
    {
        await SafeRespondAsync(message, GetForMessage(message, localizationKey, args), "SendSuccessAsync");
    }

    public async Task SendWarningAsync(IDiscordMessage message, string localizationKey, params object[] args)
    {
        var text = GetForMessage(message, localizationKey, args);
        await SafeRespondAsync(message, FormatWithPrefix(message, WarningPrefixKey, text), "SendWarningAsync");
    }

    public async Task SendErrorAsync(IDiscordMessage message, string localizationKey, params object[] args)
    {
        var text = GetForMessage(message, localizationKey, args);
        await SafeRespondAsync(message, FormatWithPrefix(message, ErrorPrefixKey, text), "SendErrorAsync");
    }

    private string GetForMessage(IDiscordMessage message, string key, params object[] args)
    {
        var guild = message.Channel.Guild;
        return guild is null
            ? localization.Get(key, args)
            : localization.Get(guild.Id, key, args);
    }

    private string FormatWithPrefix(IDiscordMessage message, string prefixKey, string text)
    {
        var prefix = GetForMessage(message, prefixKey);
        return prefix == prefixKey ? text : $"{prefix}{text}";
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
