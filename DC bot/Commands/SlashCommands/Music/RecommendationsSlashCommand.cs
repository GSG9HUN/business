using DC_bot.Constants;
using System.ComponentModel;
using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace DC_bot.Commands.SlashCommands.Music;

/// <summary>Adapts /recommendations to the shared localized text pipeline.</summary>
/// <param name="executor">Runs guild validation and interaction acknowledgement.</param>
/// <param name="factory">Adapts SDK interaction context.</param>
public sealed class RecommendationsSlashCommand(ISlashCommandExecutor executor, ISlashInteractionContextFactory factory)
{

    /// <summary>Executes the recommendations command through the shared handler.</summary>
    /// <param name="context">The Discord interaction.</param>
    /// <param name="query">Optional seed query; omit to use the current track.</param>
    /// <returns>The shared command execution.</returns>
    [Command(CommandNames.Recommendations)]
    [Description("Show recommended tracks")]
    public Task Recommendations(
        SlashCommandContext context,
        [Parameter("query")][Description("Optional seed query")] string? query = null)
    {
        return ExecuteAsync(factory.Create(context), query);
    }

    /// <summary>Routes typed options through the shared text-command pipeline.</summary>
    public Task ExecuteAsync(ISlashInteractionContext context, string? argument)
    {
        return executor.ExecuteAsync(new SlashCommandExecutionRequest(CommandNames.Recommendations, context, argument,
            RequireGuild: true, Defer: true, EnsureDeferredResponse: true), default);
    }
}
