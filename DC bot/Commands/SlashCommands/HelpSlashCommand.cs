using DC_bot.Interface;
using DC_bot.Logging;
using DC_bot.Helper;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.SlashCommands;

public abstract class HelpSlashCommand : ApplicationCommandModule
{
    private const string CommandNameHelp = "help";
    
    // Property injection supported by DSharpPlus SlashCommands
    public ILogger<PlaySlashCommand> Logger { private get; set; } = null!;
    public IEnumerable<ICommand> Commands { private get; set; } = null!;

    [SlashCommand("help", "List the available commands")]
    public async Task Help(InteractionContext ctx)
    {
        Logger.CommandInvoked(CommandNameHelp);
        //await SlashCommandResponseHelper.DeferAsync(ctx);

        var response = Commands.Aggregate(String.Empty,
            (current, command) => current + $"{command.Name} : {command.Description}\n");

        //await SlashCommandResponseHelper.EditResponseAsync(ctx, $"Available commands:\n{response}");

        Logger.CommandExecuted(CommandNameHelp);
    }
}