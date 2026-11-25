using DC_bot.Constants;
using System.ComponentModel;
using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace DC_bot.Commands.SlashCommands.Music;

/// <summary>Adapts /playnext to the existing shared text-command execution pipeline.</summary>
/// <param name="slashCommandExecutor">Executes the shared command with interaction acknowledgement.</param>
/// <param name="contextFactory">Adapts the SDK interaction context.</param>
public sealed class PlayNextSlashCommand(
    ISlashCommandExecutor slashCommandExecutor,
    ISlashInteractionContextFactory contextFactory)
{
    private const string CommandName = CommandNames.PlayNext;

    /// <summary>Queue music to play next through the shared command handler.</summary>
    /// <param name="context">The SDK slash interaction.</param>
    /// <param name="query">The source URL or search query.</param>
    /// <returns>The asynchronous shared command execution.</returns>
    [Command(CommandNames.PlayNext)]
    [Description("Queue music immediately after the current track, without skipping it")]
    public Task PlayNext(
        SlashCommandContext context,
        [Parameter("query")][Description("Queue music to play next")] string query)
    {
        return ExecuteAsync(contextFactory.Create(context), query);
    }

    /// <summary>Routes the slash request through existing guild validation and deferred acknowledgement.</summary>
    /// <param name="context">The adapted slash interaction.</param>
    /// <param name="argument">The argument passed to the shared text command.</param>
    /// <returns>The asynchronous shared command execution.</returns>
    public Task ExecuteAsync(ISlashInteractionContext context, string? argument)
    {
        return slashCommandExecutor.ExecuteAsync(
            new SlashCommandExecutionRequest(CommandName, context, argument,
                RequireGuild: true, Defer: true, EnsureDeferredResponse: true), default);
    }
}
