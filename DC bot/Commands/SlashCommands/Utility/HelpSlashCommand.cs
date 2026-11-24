using DC_bot.Constants;
using System.ComponentModel;
using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace DC_bot.Commands.SlashCommands.Utility;

/// <summary>
/// Adapts the help slash command to shared command execution and interaction acknowledgement.
/// </summary>
/// <param name="slashCommandExecutor">The slash command executor dependency used by this boundary.</param>
/// <param name="contextFactory">The slash interaction context factory dependency used by this boundary.</param>
public class HelpSlashCommand(ISlashCommandExecutor slashCommandExecutor, ISlashInteractionContextFactory contextFactory)
{
    private const string CommandName = CommandNames.Help;

    /// <summary>
    /// Adapts the help slash route to its shared handler and interaction response policy.
    /// </summary>
    /// <param name="context">The context supplied for this operation.</param>
    /// <param name="command">The optional registered command name for detailed help.</param>
    /// <returns>A task representing completion of this operation.</returns>
    [Command(CommandNames.Help)]
    [Description("List commands or show detailed command usage")]
    public Task Help(
        SlashCommandContext context,
        [Description("Command name for detailed usage; omit to list commands")] string? command = null)
    {
        return ExecuteAsync(contextFactory.Create(context), command);
    }

    /// <summary>
    /// Routes the slash interaction through the shared command executor and acknowledgement policy.
    /// </summary>
    /// <param name="context">The slash interaction context dependency used by this boundary.</param>
    /// <param name="command"></param>
    /// <returns>A task representing completion of this operation.</returns>
    public Task ExecuteAsync(ISlashInteractionContext context, string? command)
    {
        return slashCommandExecutor.ExecuteAsync(new SlashCommandExecutionRequest(
            CommandName, context, command, RequireGuild: true, Defer: true, EnsureDeferredResponse: true), default);
    }
}
