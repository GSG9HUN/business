using DC_bot.Constants;
using System.ComponentModel;
using System.Globalization;
using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace DC_bot.Commands.SlashCommands.Music;

/// <summary>Adapts lyrics query and pagination to the shared text pipeline.</summary>
/// <param name="executor">Executes localized shared commands.</param>
/// <param name="factory">Adapts SDK interaction context.</param>
public sealed class LyricsSlashCommand(ISlashCommandExecutor executor, ISlashInteractionContextFactory factory)
{

    /// <summary>Looks up a lyrics page without modifying playback.</summary>
    /// <param name="context">The Discord interaction.</param>
    /// <param name="query">Optional track query; omit to use the current track.</param>
    /// <param name="page">The one-based lyrics page.</param>
    /// <returns>The shared command execution.</returns>
    [Command(CommandNames.Lyrics)]
    [Description("Show lyrics for the current track or a query")]
    public Task Lyrics(
        SlashCommandContext context,
        [Parameter("query")][Description("Optional track query")] string? query = null,
        [Parameter("page")][Description("Lyrics page, starting at 1")] int page = 1)
    {
        return ExecuteAsync(factory.Create(context), (query ?? string.Empty) + " --page " + page.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>Routes lyrics arguments through existing deferred acknowledgement and guild validation.</summary>
    public Task ExecuteAsync(ISlashInteractionContext context, string argument)
    {
        return executor.ExecuteAsync(new SlashCommandExecutionRequest(CommandNames.Lyrics, context, argument,
            RequireGuild: true, Defer: true, EnsureDeferredResponse: true), default);
    }
}
