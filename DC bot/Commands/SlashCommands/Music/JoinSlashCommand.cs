using System.ComponentModel;
using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace DC_bot.Commands.SlashCommands.Music;

/// <summary>
/// Adapts the join slash command to shared command execution and interaction acknowledgement.
/// </summary>
/// <param name="slashCommandExecutor">The slash command executor dependency used by this boundary.</param>
/// <param name="contextFactory">The slash interaction context factory dependency used by this boundary.</param>
public class JoinSlashCommand(ISlashCommandExecutor slashCommandExecutor, ISlashInteractionContextFactory contextFactory)
{
    private const string CommandName = "join";

    /// <summary>
    /// Adapts the join slash route to its shared handler and interaction response policy.
    /// </summary>
    /// <param name="context">The context supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    [Command("join")]
    [Description("Join the voice channel and start the queued music")]
    public Task Join(SlashCommandContext context)
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
