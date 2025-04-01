using DC_bot.Interface;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class JoinCommand(
    ILavaLinkService lavaLinkService,
    IUserValidationService userValidation,
    ILogger<JoinCommand> logger,
    ILocalizationService localizationService) : ICommand
{
    public string Name => "join";
    public string Description => localizationService.Get("join_command_description");
    public async Task ExecuteAsync(IDiscordMessage message)
    {
        var validationResult = await userValidation.ValidateUserAsync(message);

        if (validationResult.IsValid is false)
        {
            return;
        }
        
        await lavaLinkService.StartPlayingQueue(validationResult.Member?.VoiceState!.Channel!, message.Channel);
        
        logger.LogInformation("Join command executed!");
    }
}