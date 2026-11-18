using System.ComponentModel;
using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace DC_bot.Commands.SlashCommands.Music;

/// <summary>Adapts /autoplay to the shared localized text pipeline.</summary>
/// <param name="executor">Runs guild validation and interaction acknowledgement.</param>
/// <param name="factory">Adapts SDK interaction context.</param>
public sealed class AutoplaySlashCommand(ISlashCommandExecutor executor, ISlashInteractionContextFactory factory)
{

    /// <summary>Executes the autoplay command through the shared handler.</summary>
    /// <param name="context">The Discord interaction.</param>
    /// <param name="mode">on, off, or status.</param>
    /// <returns>The shared command execution.</returns>
    [Command("autoplay")]
    [Description("Configure autoplay (on/off/status)")]
    public Task Autoplay(
        SlashCommandContext context,
        [Parameter("mode")][Description("on, off, or status")] string? mode = null)
    {
        return ExecuteAsync(factory.Create(context), mode);
    }

    /// <summary>Routes typed options through the shared text-command pipeline.</summary>
    public Task ExecuteAsync(ISlashInteractionContext context, string? argument)
    {
        return executor.ExecuteAsync(new SlashCommandExecutionRequest("autoplay", context, argument,
            RequireGuild: true, Defer: true, EnsureDeferredResponse: true), default);
    }
}
