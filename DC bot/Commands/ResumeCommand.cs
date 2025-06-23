using DC_bot.Interface;
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
    public string Description => localizationService.Get("resume_command_description");

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        var validationResult = await userValidation.ValidateUserAsync(message);
        
        if (validationResult.IsValid is false)
        {
            await responseBuilder.SendValidationErrorAsync(message, validationResult.ErrorKey);
            return;
        }
        
        await lavaLinkService.ResumeAsync(message, validationResult.Member);
        logger.LogInformation("Resume command executed!");
    }
}