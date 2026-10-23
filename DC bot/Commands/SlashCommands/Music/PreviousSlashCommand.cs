using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using System.ComponentModel;

namespace DC_bot.Commands.SlashCommands.Music;

/// <summary>
/// Adapts the previous slash command to shared command execution and interaction acknowledgement.
/// </summary>
/// <param name="slashCommandExecutor">The slash command executor dependency used by this boundary.</param>
/// <param name="contextFactory">The slash interaction context factory dependency used by this boundary.</param>
public class PreviousSlashCommand(ISlashCommandExecutor slashCommandExecutor, ISlashInteractionContextFactory contextFactory)
{
    private const string CommandName = "previous";

    /// <summary>
    /// Adapts the previous slash route to its shared handler and interaction response policy.
    /// </summary>
    /// <param name="context">The context supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    [Command("previous")]
    [Description("Play the previous track")]
    public Task Previous(SlashCommandContext context)
    {
        return ExecuteAsync(contextFactory.Create(context));
    }

    /// <summary>
    /// Routes the slash interaction through the shared command executor and acknowledgement policy.
    /// </summary>
    /// <param name="context">The slash interaction context dependency used by this boundary.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public Task ExecuteAsync(ISlashInteractionContext context)
    {
        return slashCommandExecutor.ExecuteAsync(new SlashCommandExecutionRequest(CommandName, context, RequireGuild: true, Defer: true, EnsureDeferredResponse: true), default);
    }
}
