using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.TextCommands.Music;

public class SkipCommand(
    ILavaLinkService lavaLinkService,
    IUserValidationService userValidation,
    ILogger<SkipCommand> logger,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    ICommandHelper commandHelper) : ICommand
{
    public string Name => "skip";
    public string Description => localizationService.Get(LocalizationKeys.SkipCommandDescription);

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);
        var validationResult = await commandHelper.TryValidateUserAsync(userValidation, responseBuilder, message);
        if (validationResult is null) return;
        
        var result = await lavaLinkService.SkipAsync(message, validationResult.Member);
        
        if (!result.Success)
        {
            await responseBuilder.SendValidationErrorAsync(message, result.ErrorCode ?? ValidationErrorKeys.LavalinkError);
            return;
        }
        
        await responseBuilder.SendSuccessAsync(message, LocalizationKeys.SkipCommandResponse);
        logger.CommandExecuted(Name);
    }
}