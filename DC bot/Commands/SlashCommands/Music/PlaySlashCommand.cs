using DC_bot.Constants;
using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using System.ComponentModel;

namespace DC_bot.Commands.SlashCommands.Music;

/// <summary>
/// Adapts the play slash command to shared command execution and interaction acknowledgement.
/// </summary>
/// <param name="slashCommandExecutor">The slash command executor dependency used by this boundary.</param>
/// <param name="contextFactory">The slash interaction context factory dependency used by this boundary.</param>
public class PlaySlashCommand(ISlashCommandExecutor slashCommandExecutor, ISlashInteractionContextFactory contextFactory)
{
    private const string CommandName = CommandNames.Play;

    /// <summary>
    /// Adapts the play slash route to its shared handler and interaction response policy.
    /// </summary>
    /// <param name="context">The context supplied for this operation.</param>
    /// <param name="query">The query supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    [Command(CommandNames.Play)]
    [Description("Add music from a query or URL to the queue; start playback if nothing is playing")]
    public Task Play(SlashCommandContext context, [Parameter("query")][Description("URL or search query")] string query)
    {
        return ExecuteAsync(contextFactory.Create(context), query);
    }

    /// <summary>
    /// Routes the slash interaction through the shared command executor and acknowledgement policy.
    /// </summary>
    /// <param name="context">The slash interaction context dependency used by this boundary.</param>
    /// <param name="query">The query supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public Task ExecuteAsync(ISlashInteractionContext context, string query)
    {
        return slashCommandExecutor.ExecuteAsync(new SlashCommandExecutionRequest(CommandName, context, query, RequireGuild: true, Defer: true, EnsureDeferredResponse: true), default);
    }
}
