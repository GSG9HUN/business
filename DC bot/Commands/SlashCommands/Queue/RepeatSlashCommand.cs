using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using System.ComponentModel;

namespace DC_bot.Commands.SlashCommands.Queue;

[Command("repeat")]
[Description("Control repeat mode")]
public class RepeatSlashCommand(
    ISlashCommandExecutor slashCommandExecutor,
    ISlashInteractionContextFactory contextFactory)
{
    [Command("track")]
    [Description("Toggle repeat for the current track")]
    public Task Track(SlashCommandContext context)
    {
        return ExecuteTrackAsync(contextFactory.Create(context));
    }

    [Command("list")]
    [Description("Toggle repeat for the current queue")]
    public Task List(SlashCommandContext context)
    {
        return ExecuteListAsync(contextFactory.Create(context));
    }

    public Task ExecuteTrackAsync(ISlashInteractionContext context)
    {
        return ExecuteAsync("repeat", context);
    }

    public Task ExecuteListAsync(ISlashInteractionContext context)
    {
        return ExecuteAsync("repeatList", context);
    }

    private Task ExecuteAsync(string commandName, ISlashInteractionContext context)
    {
        return slashCommandExecutor.ExecuteAsync(new SlashCommandExecutionRequest(
            commandName,
            context,
            RequireGuild: true,
            Defer: true,
            EnsureDeferredResponse: true));
    }
}
