using DC_bot.Constants;
using DC_bot.Interface;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class PingCommand(
    IUserValidationService userValidation,
    ILogger<PingCommand> logger,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService) : ICommand
{
    public string Name => "ping";
    public string Description => localizationService.Get(LocalizationKeys.PingCommandDescription);

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        if (userValidation.IsBotUser(message))
        {
            return;
        }

        await responseBuilder.SendSuccessAsync(message, LocalizationKeys.PingCommandResponse);
        logger.LogInformation("Ping command executed!");
    }
}