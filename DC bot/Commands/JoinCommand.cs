using DC_bot.Constants;
using DC_bot.Interface;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class JoinCommand(
    ILavaLinkService lavaLinkService,
    IUserValidationService userValidation,
    ILogger<JoinCommand> logger,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService) : ICommand
{
    public string Name => "join";
    public string Description => localizationService.Get(LocalizationKeys.JoinCommandDescription);
    public async Task ExecuteAsync(IDiscordMessage message)
    {
        var validationResult = await userValidation.ValidateUserAsync(message);

        if (validationResult.IsValid is false)
        {
            await responseBuilder.SendValidationErrorAsync(message, validationResult.ErrorKey);
            return;
        }

        await lavaLinkService.StartPlayingQueue(message, message.Channel, validationResult.Member);

        logger.LogInformation("Join command executed!");
    }
}