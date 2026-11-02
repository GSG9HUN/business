using DC_bot.Constants;
using DC_bot.Exceptions.Localization;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Persistence.Models.MobileAppUserSettings;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.TextCommands.Utility;

/// <summary>
/// Handles the language text command through validation, service, and localized response boundaries.
/// </summary>
/// <param name="logger">The structured logger used at this operation boundary.</param>
/// <param name="userValidation">The user validation service dependency used by this boundary.</param>
/// <param name="responseBuilder">The response builder dependency used by this boundary.</param>
/// <param name="localizationService">The localization service dependency used by this boundary.</param>
/// <param name="commandHelper">The command helper dependency used by this boundary.</param>
public class LanguageCommand(
    ILogger<LanguageCommand> logger,
    IUserValidationService userValidation,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    ICommandHelper commandHelper) : ICommand
{

    /// <inheritdoc />
    public string Name => "language";

    /// <inheritdoc />
    public string Description => localizationService.Get(LocalizationKeys.LanguageCommandDescription);

    /// <summary>
    /// Validates the message context, performs this command&apos;s action, and sends its localized outcome.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);

        if (userValidation.IsBotUser(message))
        {
            return;
        }

        var language = await commandHelper.TryGetArgumentAsync(message, responseBuilder, logger, Name, default);

        if (language is null)
        {
            return;
        }

        if (!MobileAppLanguageCode.TryNormalize(language, out language))
        {
            await responseBuilder.SendValidationErrorAsync(message, LocalizationKeys.LanguageCommandInvalidLanguage, default);
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
