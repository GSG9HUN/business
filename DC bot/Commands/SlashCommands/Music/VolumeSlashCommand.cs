using DC_bot.Constants;
using System.Globalization;
using System.ComponentModel;
using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace DC_bot.Commands.SlashCommands.Music;

/// <summary>Adapts /volume to the existing shared text-command execution pipeline.</summary>
/// <param name="slashCommandExecutor">Executes the shared command with interaction acknowledgement.</param>
/// <param name="contextFactory">Adapts the SDK interaction context.</param>
public sealed class VolumeSlashCommand(
    ISlashCommandExecutor slashCommandExecutor,
    ISlashInteractionContextFactory contextFactory)
{
    private const string CommandName = CommandNames.Volume;

    /// <summary>Show or set player output volume (0-200%) through the shared command handler.</summary>
    /// <param name="context">The SDK slash interaction.</param>
    /// <param name="value">The value passed to the shared text command.</param>
    /// <returns>The asynchronous shared command execution.</returns>
    [Command(CommandNames.Volume)]
    [Description("Show or set player output volume (0-200%)")]
    public Task Volume(
        SlashCommandContext context,
        [Parameter("value")][Description("Show or set player output volume (0-200%)")] int? value = null)
    {
        return ExecuteAsync(contextFactory.Create(context), value?.ToString(CultureInfo.InvariantCulture));
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
