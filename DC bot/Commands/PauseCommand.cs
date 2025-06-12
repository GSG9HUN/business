using DC_bot.Interface;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class PauseCommand(
    ILavaLinkService lavaLinkService,
    IUserValidationService userValidation,
    ILogger<PauseCommand> logger,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService) : ICommand
{
    public string Name => "pause";
    public string Description => localizationService.Get("pause_command_description");

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        var validationResult = await userValidation.ValidateUserAsync(message);
        
        if (validationResult.IsValid is false)
        {
            await responseBuilder.SendValidationErrorAsync(message, validationResult.ErrorKey);
            return;
        }

        await lavaLinkService.PauseAsync(message, validationResult.Member);
        logger.LogInformation("Pause command executed!");
    }
}