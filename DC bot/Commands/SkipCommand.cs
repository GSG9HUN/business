using DC_bot.Interface;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class SkipCommand(ILavaLinkService lavaLinkService, IUserValidationService userValidation, ILogger<SkipCommand> logger) : ICommand
{
    public string Name => "skip";
    public string Description => "Skip the current track.";

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        var validationResult = await userValidation.ValidateUserAsync(message);
        
        if (validationResult.IsValid is false)
        {
            return;
        }

        await lavaLinkService.SkipAsync(message.Channel);
        logger.LogInformation("Skip command executed!");
    }
}