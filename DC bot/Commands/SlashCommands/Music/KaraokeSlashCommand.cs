using System.ComponentModel;
using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace DC_bot.Commands.SlashCommands.Music;

/// <summary>Routes /karaoke through the shared text-command pipeline.</summary>
/// <param name="slashCommandExecutor">Executes the shared command with deferred acknowledgement.</param>
/// <param name="contextFactory">Adapts the SDK context.</param>
public sealed class KaraokeSlashCommand(
    ISlashCommandExecutor slashCommandExecutor,
    ISlashInteractionContextFactory contextFactory)
{

    /// <summary>Show or toggle karaoke: on/off.</summary>
    /// <param name="context">The SDK interaction context.</param>
    /// <param name="mode">The optional value; omitted values inspect the current state.</param>
    /// <returns>The asynchronous shared command execution.</returns>
    [Command("karaoke")]
    [Description("Show or toggle karaoke: on/off")]
    public Task Karaoke(
        SlashCommandContext context,
        [Parameter("mode")][Description("Show or toggle karaoke: on/off")] string? mode = null)
    {
        return ExecuteAsync(contextFactory.Create(context), mode);
    }

    /// <summary>Runs the localized shared command after guild validation and acknowledgement.</summary>
    /// <param name="context">The adapted interaction context.</param>
    /// <param name="argument">The invariant argument or null for inspection.</param>
    /// <returns>The asynchronous shared command execution.</returns>
    public Task ExecuteAsync(ISlashInteractionContext context, string? argument)
    {
        return slashCommandExecutor.ExecuteAsync(new SlashCommandExecutionRequest(
            "karaoke", context, argument, RequireGuild: true, Defer: true, EnsureDeferredResponse: true), default);
    }
}
