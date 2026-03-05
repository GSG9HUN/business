using DC_bot.Constants;
using DC_bot.Helper;
using DC_bot.Interface;
using DC_bot.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class HelpCommand(
    IUserValidationService userValidation,
    ILogger<HelpCommand> logger,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    IServiceProvider serviceProvider) : ICommand
{
    public string Name => "help";
    public string Description => localizationService.Get(LocalizationKeys.HelpCommandDescription);

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);
        if (CommandValidationHelper.IsBotUser(userValidation, message))
        {
            return;
        }
        
        var commands = serviceProvider.GetServices<ICommand>();
        var response = commands.Aggregate(string.Empty,
            (current, command) => current + $"{command.Name} : {command.Description}\n");

        await responseBuilder.SendSuccessAsync(message,
            $"{localizationService.Get(LocalizationKeys.HelpCommandResponse)}\n{response}");
        logger.CommandExecuted(Name);
    }
}