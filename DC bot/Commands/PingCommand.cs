using DC_bot.Interface;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class PingCommand(IUserValidationService userValidation,ILogger<PingCommand> logger) : ICommand
{
    public string Name => "ping";
    public string Description => "Answer with pong!";

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