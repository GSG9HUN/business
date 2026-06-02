using System.ComponentModel;
using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace DC_bot.Commands.SlashCommands.Music;

public class PauseSlashCommand(
    ISlashCommandExecutor slashCommandExecutor,
    ISlashInteractionContextFactory contextFactory)
{
    private const string CommandName = "pause";

    [Command("pause")]
    [Description("Pause the current music")]
    public Task Pause(SlashCommandContext context)
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
