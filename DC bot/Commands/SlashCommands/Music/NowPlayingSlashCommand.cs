using System.ComponentModel;
using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace DC_bot.Commands.SlashCommands.Music;

/// <summary>Adapts /nowplaying to the existing shared text-command execution pipeline.</summary>
/// <param name="slashCommandExecutor">Executes the shared command with interaction acknowledgement.</param>
/// <param name="contextFactory">Adapts the SDK interaction context.</param>
public sealed class NowPlayingSlashCommand(
    ISlashCommandExecutor slashCommandExecutor,
    ISlashInteractionContextFactory contextFactory)
{
    private const string CommandName = "nowplaying";

    /// <summary>Show the current track and playback details through the shared command handler.</summary>
    /// <param name="context">The SDK slash interaction.</param>
    /// <returns>The asynchronous shared command execution.</returns>
    [Command("nowplaying")]
    [Description("Show the current track and playback details")]
    public Task NowPlaying(
        SlashCommandContext context)
    {
        return ExecuteAsync(contextFactory.Create(context));
    }

    /// <summary>Routes the slash request through existing guild validation and deferred acknowledgement.</summary>
    /// <param name="context">The adapted slash interaction.</param>
    /// <returns>The asynchronous shared command execution.</returns>
    public Task ExecuteAsync(ISlashInteractionContext context)
    {
        return slashCommandExecutor.ExecuteAsync(
            new SlashCommandExecutionRequest(CommandName, context,
                RequireGuild: true, Defer: true, EnsureDeferredResponse: true), default);
    }
}
