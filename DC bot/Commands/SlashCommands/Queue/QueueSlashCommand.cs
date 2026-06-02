using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using System.ComponentModel;

namespace DC_bot.Commands.SlashCommands.Queue;

public class QueueSlashCommand(
    ISlashCommandExecutor slashCommandExecutor,
    ISlashInteractionContextFactory contextFactory)
{
    private const string CommandName = "viewList";

    [Command("queue")]
    [Description("View the current queue")]
    public Task Queue(SlashCommandContext context)
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
