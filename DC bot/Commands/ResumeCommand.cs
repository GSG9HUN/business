using DC_bot.Constants;
using DC_bot.Helper;
using DC_bot.Interface;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class ResumeCommand(
    ILavaLinkService lavaLinkService,
    IUserValidationService userValidation,
    ILogger<ResumeCommand> logger,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService) : ICommand
{
    public string Name => "resume";
    public string Description => localizationService.Get(LocalizationKeys.ResumeCommandDescription);

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);
        var validationResult = await CommandValidationHelper.TryValidateUserAsync(userValidation, responseBuilder, message);
        if (validationResult is null) return;

        await lavaLinkService.ResumeAsync(message, validationResult.Member);
        logger.CommandExecuted(Name);
    }
}