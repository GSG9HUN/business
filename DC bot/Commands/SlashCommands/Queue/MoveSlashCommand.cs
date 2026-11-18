using System.ComponentModel;
using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace DC_bot.Commands.SlashCommands.Queue;

/// <summary>Adapts /move to the existing shared text-command execution pipeline.</summary>
/// <param name="slashCommandExecutor">Executes the shared command with interaction acknowledgement.</param>
/// <param name="contextFactory">Adapts the SDK interaction context.</param>
public sealed class MoveSlashCommand(
    ISlashCommandExecutor slashCommandExecutor,
    ISlashInteractionContextFactory contextFactory)
{
    private const string CommandName = "move";

    /// <summary>Handle move queue operation through the shared command handler.</summary>
    /// <param name="context">The SDK slash interaction.</param>
    /// <param name="from">The one-based source waiting position.</param>
    /// <param name="to">The one-based destination waiting position.</param>
    /// <returns>The asynchronous shared command execution.</returns>
    [Command("move")]
    [Description("Handle move queue operation")]
    public Task Move(
        SlashCommandContext context,
        [Parameter("from")][Description("One-based waiting position")] int from,
        [Parameter("to")][Description("New one-based waiting position")] int to)
    {
        return ExecuteAsync(contextFactory.Create(context), $"{from} {to}");
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
