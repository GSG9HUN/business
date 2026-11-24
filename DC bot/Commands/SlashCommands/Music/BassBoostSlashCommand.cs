using DC_bot.Constants;
using System.ComponentModel;
using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace DC_bot.Commands.SlashCommands.Music;

/// <summary>Routes /bassboost through the shared text-command pipeline.</summary>
/// <param name="slashCommandExecutor">Executes the shared command with deferred acknowledgement.</param>
/// <param name="contextFactory">Adapts the SDK context.</param>
public sealed class BassBoostSlashCommand(
    ISlashCommandExecutor slashCommandExecutor,
    ISlashInteractionContextFactory contextFactory)
{

    /// <summary>Show or change bass emphasis: off, low, medium, high.</summary>
    /// <param name="context">The SDK interaction context.</param>
    /// <param name="level">The optional value; omitted values inspect the current state.</param>
    /// <returns>The asynchronous shared command execution.</returns>
    [Command(CommandNames.BassBoost)]
    [Description("Show or change bass emphasis: off, low, medium, high")]
    public Task BassBoost(
        SlashCommandContext context,
        [Parameter("level")][Description("Show or change bass emphasis: off, low, medium, high")] string? level = null)
    {
        return ExecuteAsync(contextFactory.Create(context), level);
    }

    /// <summary>Runs the localized shared command after guild validation and acknowledgement.</summary>
    /// <param name="context">The adapted interaction context.</param>
    /// <param name="argument">The invariant argument or null for inspection.</param>
    /// <returns>The asynchronous shared command execution.</returns>
    public Task ExecuteAsync(ISlashInteractionContext context, string? argument)
    {
        return slashCommandExecutor.ExecuteAsync(new SlashCommandExecutionRequest(
            "bassboost", context, argument, RequireGuild: true, Defer: true, EnsureDeferredResponse: true), default);
    }
}
