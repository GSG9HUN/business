using System.Globalization;
using System.ComponentModel;
using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace DC_bot.Commands.SlashCommands.Music;

/// <summary>Routes /speed through the shared text-command pipeline.</summary>
/// <param name="slashCommandExecutor">Executes the shared command with deferred acknowledgement.</param>
/// <param name="contextFactory">Adapts the SDK context.</param>
public sealed class SpeedSlashCommand(
    ISlashCommandExecutor slashCommandExecutor,
    ISlashInteractionContextFactory contextFactory)
{

    /// <summary>Show or change speed (0.5-2.0).</summary>
    /// <param name="context">The SDK interaction context.</param>
    /// <param name="multiplier">The optional value; omitted values inspect the current state.</param>
    /// <returns>The asynchronous shared command execution.</returns>
    [Command("speed")]
    [Description("Show or change speed (0.5-2.0)")]
    public Task Speed(
        SlashCommandContext context,
        [Parameter("multiplier")][Description("Show or change speed (0.5-2.0)")] double? multiplier = null)
    {
        return ExecuteAsync(contextFactory.Create(context), multiplier?.ToString("R", CultureInfo.InvariantCulture));
    }

    /// <summary>Runs the localized shared command after guild validation and acknowledgement.</summary>
    /// <param name="context">The adapted interaction context.</param>
    /// <param name="argument">The invariant argument or null for inspection.</param>
    /// <returns>The asynchronous shared command execution.</returns>
    public Task ExecuteAsync(ISlashInteractionContext context, string? argument)
    {
        return slashCommandExecutor.ExecuteAsync(new SlashCommandExecutionRequest(
            "speed", context, argument, RequireGuild: true, Defer: true, EnsureDeferredResponse: true), default);
    }
}
