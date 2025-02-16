using DC_bot.Interface;
using DC_bot.Service;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class HelpCommand(IUserValidationService userValidation, ILogger<HelpCommand> logger) : ICommand
{
    public string Name => "help";
    public string Description => "Lists all available commands.";

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        if (userValidation.IsBotUser(message))
        {
            return;
        }
        
        var commands = ServiceLocator.GetServices<ICommand>();
        var response = commands.Aggregate(string.Empty,
            (current, command) => current + $"{command.Name} : {command.Description}\n");

        await message.RespondAsync($"Available commands:\n{response}");
        logger.LogInformation("Help Command executed!");
    }
}