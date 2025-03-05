using DC_bot.Interface;
using DC_bot.Service;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class HelpCommand(
    IUserValidationService userValidation,
    ILogger<HelpCommand> logger,
    ILocalizationService localizationService) : ICommand
{
    public string Name => "help";
    public string Description => localizationService.Get("help_command_description");

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        if (userValidation.IsBotUser(message))
        {
            return;
        }

        var commands = ServiceLocator.GetServices<ICommand>();
        var response = commands.Aggregate(string.Empty,
            (current, command) => current + $"{command.Name} : {command.Description}\n");

        await message.RespondAsync($"{localizationService.Get("help_command_response")}\n{response}");
        logger.LogInformation("Help Command executed!");
    }
}