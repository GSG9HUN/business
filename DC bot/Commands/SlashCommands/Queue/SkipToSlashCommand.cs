using DC_bot.Constants;
using System.ComponentModel;
using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace DC_bot.Commands.SlashCommands.Queue;

/// <summary>Adapts /skipto to the existing shared text-command execution pipeline.</summary>
/// <param name="slashCommandExecutor">Executes the shared command with interaction acknowledgement.</param>
/// <param name="contextFactory">Adapts the SDK interaction context.</param>
public sealed class SkipToSlashCommand(
    ISlashCommandExecutor slashCommandExecutor,
    ISlashInteractionContextFactory contextFactory)
{
    private const string CommandName = CommandNames.SkipTo;

    /// <summary>Handle skipto queue operation through the shared command handler.</summary>
    /// <param name="context">The SDK slash interaction.</param>
    /// <param name="position">The position passed to the shared text command.</param>
    /// <returns>The asynchronous shared command execution.</returns>
    [Command(CommandNames.SkipTo)]
    [Description("Start a waiting track by position, skipping earlier waiting tracks for this pass")]
    public Task SkipTo(
        SlashCommandContext context,
        [Parameter("position")][Description("One-based waiting position")] int position)
    {
        return ExecuteAsync(contextFactory.Create(context), position.ToString(System.Globalization.CultureInfo.InvariantCulture));
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
