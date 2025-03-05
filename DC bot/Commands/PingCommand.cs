using DC_bot.Interface;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class PingCommand(
    IUserValidationService userValidation,
    ILogger<PingCommand> logger,
    ILocalizationService localizationService) : ICommand
{
    public string Name => "ping";
    public string Description => localizationService.Get("ping_command_description");

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        if (userValidation.IsBotUser(message))
        {
            return;
        }

        await message.RespondAsync("Pong!");
        logger.LogInformation("Ping command executed!");
    }
}