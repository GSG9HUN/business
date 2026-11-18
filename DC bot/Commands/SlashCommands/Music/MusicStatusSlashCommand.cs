using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using System.ComponentModel;

namespace DC_bot.Commands.SlashCommands.Music;

/// <summary>
/// Adapts the music status slash command to shared command execution and interaction acknowledgement.
/// </summary>
/// <param name="slashCommandExecutor">The slash command executor dependency used by this boundary.</param>
/// <param name="contextFactory">The slash interaction context factory dependency used by this boundary.</param>
public class MusicStatusSlashCommand(ISlashCommandExecutor slashCommandExecutor, ISlashInteractionContextFactory contextFactory)
{
    private const string CommandName = "music-status";

    /// <summary>
    /// Adapts the music status slash route to its shared handler and interaction response policy.
    /// </summary>
    /// <param name="context">The context supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    [Command("music-status")]
    [Description("Show playback, voice and guild control status")]
    public Task MusicStatus(SlashCommandContext context)
    {
        return ExecuteAsync(contextFactory.Create(context));
    }

    /// <summary>
    /// Routes the slash interaction through the shared command executor and acknowledgement policy.
    /// </summary>
    /// <param name="context">The slash interaction context dependency used by this boundary.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public Task ExecuteAsync(ISlashInteractionContext context)
    {
        return slashCommandExecutor.ExecuteAsync(new SlashCommandExecutionRequest(CommandName, context, RequireGuild: true, Defer: true, EnsureDeferredResponse: true), CancellationToken.None);
    }
}
