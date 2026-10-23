using System.ComponentModel;
using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace DC_bot.Commands.SlashCommands.Music;

/// <summary>
/// Adapts the twenty-four-seven slash command to the shared text command implementation.
/// </summary>
/// <param name="slashCommandExecutor">Dispatches the registered command and handles acknowledgement.</param>
/// <param name="contextFactory">Creates the shared interaction context.</param>
public sealed class TwentyFourSevenSlashCommand(
    ISlashCommandExecutor slashCommandExecutor,
    ISlashInteractionContextFactory contextFactory)
{
    private const string CommandName = "24-7";

    /// <summary>
    /// Enables or disables persistent voice mode through a slash interaction.
    /// </summary>
    /// <param name="context">The SDK slash interaction context.</param>
    /// <param name="enabled">Whether persistent voice mode should be enabled.</param>
    /// <returns>A task that completes after command execution and response processing.</returns>
    [Command("24-7")]
    [Description("Enable or disable twenty-four-seven voice mode")]
    public Task TwentyFourSeven(
        SlashCommandContext context,
        [Parameter("enabled")]
        [Description("Whether twenty-four-seven mode should be enabled")]
        bool enabled)
    {
        return ExecuteAsync(
            contextFactory.Create(context),
            enabled);
    }

    /// <summary>
    /// Forwards an explicit boolean argument to the shared command executor.
    /// </summary>
    /// <param name="context">The shared interaction context.</param>
    /// <param name="enabled">The requested persistent voice mode.</param>
    /// <param name="cancellationToken">Cancels supported interaction processing.</param>
    /// <returns>A task that completes after the shared command finishes.</returns>
    public Task ExecuteAsync(
        ISlashInteractionContext context,
        bool enabled,
        CancellationToken cancellationToken = default)
    {
        var request = new SlashCommandExecutionRequest(
            CommandName,
            context,
            Argument: enabled ? "true" : "false",
            RequireGuild: true,
            Defer: true,
            EnsureDeferredResponse: true);

        return slashCommandExecutor.ExecuteAsync(
            request,
            cancellationToken);
    }
}