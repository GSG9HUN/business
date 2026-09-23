using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using System.ComponentModel;

namespace DC_bot.Commands.SlashCommands.Music;

public class PreviousSlashCommand(
    ISlashCommandExecutor slashCommandExecutor,
    ISlashInteractionContextFactory contextFactory)
{
    private const string CommandName = "previous";

    [Command("previous")]
    [Description("Play the previous track")]
    public Task Previous(SlashCommandContext context)
    {
        return ExecuteAsync(contextFactory.Create(context));
    }

    public Task ExecuteAsync(ISlashInteractionContext context)
    {
        return slashCommandExecutor.ExecuteAsync(new SlashCommandExecutionRequest(
            CommandName,
            context,
            RequireGuild: true,
            Defer: true,
            EnsureDeferredResponse: true));
    }
}
