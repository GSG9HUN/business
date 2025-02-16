using DC_bot.Interface;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class ResumeCommand(ILavaLinkService lavaLinkService, IUserValidationService userValidation, ILogger<ResumeCommand> logger) : ICommand
{
    public string Name => "resume";
    public string Description => "Resume the current music.";

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