using DC_bot.Interface;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class ResumeCommand(
    ILavaLinkService lavaLinkService,
    IUserValidationService userValidation,
    ILogger<ResumeCommand> logger,
    ILocalizationService localizationService) : ICommand
{
    public string Name => "resume";
    public string Description => localizationService.Get("resume_command_description");

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        var validationResult = await userValidation.ValidateUserAsync(message);

        if (validationResult.IsValid is false)
        {
            return;
        }

        await lavaLinkService.ResumeAsync(message.Channel);
        logger.LogInformation("Resume command executed!");
    }
}