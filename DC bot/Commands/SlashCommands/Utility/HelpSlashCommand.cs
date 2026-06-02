using System.ComponentModel;
using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace DC_bot.Commands.SlashCommands.Utility;

public class HelpSlashCommand(
    ISlashCommandExecutor slashCommandExecutor,
    ISlashInteractionContextFactory contextFactory)
{
    private const string CommandName = "help";

    [Command("help")]
    [Description("List the available commands")]
    public Task Help(SlashCommandContext context)
    {
        return ExecuteAsync(contextFactory.Create(context));
    }

    public Task ExecuteAsync(ISlashInteractionContext context)
    {
        return slashCommandExecutor.ExecuteAsync(new SlashCommandExecutionRequest(
            CommandName,
            context,
            RequireGuild: true));
    }
}
