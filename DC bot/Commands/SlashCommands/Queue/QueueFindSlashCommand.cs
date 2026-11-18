using System.ComponentModel;
using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace DC_bot.Commands.SlashCommands.Queue;

/// <summary>Adapts /queuefind to the existing shared text-command execution pipeline.</summary>
/// <param name="slashCommandExecutor">Executes the shared command with interaction acknowledgement.</param>
/// <param name="contextFactory">Adapts the SDK interaction context.</param>
public sealed class QueueFindSlashCommand(
    ISlashCommandExecutor slashCommandExecutor,
    ISlashInteractionContextFactory contextFactory)
{
    private const string CommandName = "queuefind";

    /// <summary>Handle queuefind queue operation through the shared command handler.</summary>
    /// <param name="context">The SDK slash interaction.</param>
    /// <param name="query">The title or artist text to find.</param>
    /// <returns>The asynchronous shared command execution.</returns>
    [Command("queuefind")]
    [Description("Handle queuefind queue operation")]
    public Task QueueFind(
        SlashCommandContext context,
        [Parameter("query")][Description("Title or artist to find")] string query)
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
