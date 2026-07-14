using DC_bot.Constants;
using DC_bot.Exceptions.Localization;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.TextCommands.Utility;

public class LanguageCommand(
    ILogger<LanguageCommand> logger,
    IUserValidationService userValidation,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    ICommandHelper commandHelper) : ICommand
{
    private static readonly HashSet<string> AllowedLanguageCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "eng",
        "hu"
    };

    public string Name => "language";
    public string Description => localizationService.Get(LocalizationKeys.LanguageCommandDescription);

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);

        if (userValidation.IsBotUser(message)) return;

        var language = await commandHelper.TryGetArgumentAsync(message, responseBuilder, logger, Name);
        
        if (language is null) return;

        language = language.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(language) || !AllowedLanguageCodes.Contains(language))
        {
            await responseBuilder.SendValidationErrorAsync(message, LocalizationKeys.LanguageCommandInvalidLanguage);
            return;
        }

        try
        {
            localizationService.SaveLanguage(message.Channel.Guild.Id, language);
        }
        catch (LocalizationException ex)
        {
            logger.CommandExecutionFailed(ex, Name);
            await responseBuilder.SendErrorAsync(message, LocalizationKeys.LanguageCommandError);
            return;
        }

        await responseBuilder.SendSuccessAsync(message, LocalizationKeys.LanguageCommandResponse);
        logger.CommandExecuted(Name);
    }
}
