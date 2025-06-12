using DC_bot.Interface;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class LeaveCommand(  
    ILavaLinkService lavaLinkService,
    IUserValidationService userValidation,
    ILogger<JoinCommand> logger,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService) : ICommand
{
    public string Name => "leave";
    public string Description => localizationService.Get("leave_command_description");
    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.LogInformation("Leave command invoked.");
        var validationResult = await userValidation.ValidateUserAsync(message);
        
        if (validationResult.IsValid is false)
        {
            await responseBuilder.SendValidationErrorAsync(message, validationResult.ErrorKey);
            return;
        }

        await lavaLinkService.LeaveVoiceChannel(message, validationResult.Member);
       
        logger.LogInformation("Leave command executed.");
    }
}