using System.ComponentModel;
using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace DC_bot.Commands.SlashCommands.Music;

/// <summary>Adapts /seek to the existing shared text-command execution pipeline.</summary>
/// <param name="slashCommandExecutor">Executes the shared command with interaction acknowledgement.</param>
/// <param name="contextFactory">Adapts the SDK interaction context.</param>
public sealed class SeekSlashCommand(
    ISlashCommandExecutor slashCommandExecutor,
    ISlashInteractionContextFactory contextFactory)
{
    private const string CommandName = "seek";

    /// <summary>Seek to seconds, mm:ss, or hh:mm:ss through the shared command handler.</summary>
    /// <param name="context">The SDK slash interaction.</param>
    /// <param name="position">The position passed to the shared text command.</param>
    /// <returns>The asynchronous shared command execution.</returns>
    [Command("seek")]
    [Description("Seek to seconds, mm:ss, or hh:mm:ss")]
    public Task Seek(
        SlashCommandContext context,
        [Parameter("position")][Description("Seek to seconds, mm:ss, or hh:mm:ss")] string position)
    {
        return ExecuteAsync(contextFactory.Create(context), position);
    }

    /// <summary>Routes the slash request through existing guild validation and deferred acknowledgement.</summary>
    /// <param name="context">The adapted slash interaction.</param>
    /// <param name="argument">The invariant text argument, or null for a volume query.</param>
    /// <returns>The asynchronous shared command execution.</returns>
    public Task ExecuteAsync(ISlashInteractionContext context, string? argument)
    {
        return slashCommandExecutor.ExecuteAsync(
            new SlashCommandExecutionRequest(CommandName, context, argument,
                RequireGuild: true, Defer: true, EnsureDeferredResponse: true), default);
    }
}
