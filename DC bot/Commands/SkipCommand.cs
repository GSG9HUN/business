using DC_bot.Constants;
using DC_bot.Interface;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class SkipCommand(
    ILavaLinkService lavaLinkService,
    IUserValidationService userValidation,
    ILogger<SkipCommand> logger,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService) : ICommand
{
    public string Name => "skip";
    public string Description => localizationService.Get(LocalizationKeys.SkipCommandDescription);

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        var validationResult = await userValidation.ValidateUserAsync(message);

        if (validationResult.IsValid is false)
        {
            await responseBuilder.SendValidationErrorAsync(message, validationResult.ErrorKey);
            return;
        }

        await lavaLinkService.SkipAsync(message, validationResult.Member);
        logger.LogInformation("Skip command executed!");
    }
}