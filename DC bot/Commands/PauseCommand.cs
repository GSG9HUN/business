using DC_bot.Interface;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class PauseCommand(ILavaLinkService lavaLinkService, IUserValidationService userValidation, ILogger<PauseCommand> logger) : ICommand
{
    public string Name => "pause";
    public string Description => "Pause the current music.";

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        var validationResult = await userValidation.ValidateUserAsync(message);
        
        if (validationResult.IsValid is false)
        {
            return;
        }

        await lavaLinkService.PauseAsync(message.Channel);
        logger.LogInformation("Pause command executed!");
    }
}