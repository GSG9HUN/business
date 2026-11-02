using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using System.ComponentModel;

namespace DC_bot.Commands.SlashCommands.Queue;

/// <summary>
/// Adapts the repeat slash command to shared command execution and interaction acknowledgement.
/// </summary>
/// <param name="slashCommandExecutor">The slash command executor dependency used by this boundary.</param>
/// <param name="contextFactory">The slash interaction context factory dependency used by this boundary.</param>
[Command("repeat")]
[Description("Control repeat mode")]
public class RepeatSlashCommand(ISlashCommandExecutor slashCommandExecutor, ISlashInteractionContextFactory contextFactory)
{

    /// <summary>
    /// Adapts the track slash route to its shared handler and interaction response policy.
    /// </summary>
    /// <param name="context">The context supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    [Command("track")]
    [Description("Toggle repeat for the current track")]
    public Task Track(SlashCommandContext context)
    {
        return ExecuteTrackAsync(contextFactory.Create(context));
    }

    /// <summary>
    /// Adapts the list slash route to its shared handler and interaction response policy.
    /// </summary>
    /// <param name="context">The context supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    [Command("list")]
    [Description("Toggle repeat for the current queue")]
    public Task List(SlashCommandContext context)
    {
        return ExecuteListAsync(contextFactory.Create(context));
    }

    /// <summary>
    /// Adapts the execute track async slash route to its shared handler and interaction response policy.
    /// </summary>
    /// <param name="context">The slash interaction context dependency used by this boundary.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public Task ExecuteTrackAsync(ISlashInteractionContext context)
    {
        return ExecuteAsync("repeat", context);
    }

    /// <summary>
    /// Adapts the execute list async slash route to its shared handler and interaction response policy.
    /// </summary>
    /// <param name="context">The slash interaction context dependency used by this boundary.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public Task ExecuteListAsync(ISlashInteractionContext context)
    {
        return ExecuteAsync("repeatList", context);
    }

    /// <summary>
    /// Routes the slash interaction through the shared command executor and acknowledgement policy.
    /// </summary>
    /// <param name="commandName">The command name supplied for this operation.</param>
    /// <param name="context">The slash interaction context dependency used by this boundary.</param>
    /// <returns>A task representing completion of this operation.</returns>
    private Task ExecuteAsync(string commandName, ISlashInteractionContext context)
    {
        return slashCommandExecutor.ExecuteAsync(new SlashCommandExecutionRequest(commandName, context, RequireGuild: true, Defer: true, EnsureDeferredResponse: true), default);
    }
}
