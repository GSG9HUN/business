using System.ComponentModel;
using System.Globalization;
using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace DC_bot.Commands.SlashCommands.Music;

/// <summary>Groups personal favorite add, list, remove and play operations.</summary>
/// <param name="executor">Executes shared localized command behavior.</param>
/// <param name="factory">Adapts SDK interaction context.</param>
[Command("favourite")]
[Description("Manage your personal favorite tracks")]
public sealed class FavouriteSlashCommand(ISlashCommandExecutor executor, ISlashInteractionContextFactory factory)
{

    /// <summary>Saves the current track or a single query result as a personal favorite.</summary>
    /// <param name="context">The Discord interaction.</param>
    /// <param name="query">Optional source query.</param>
    /// <returns>The shared command execution.</returns>
    [Command("add")]
    [Description("Save a personal favorite")]
    public Task Add(SlashCommandContext context, [Parameter("query")][Description("Optional track query")] string? query = null)
    {
        return ExecuteAsync(factory.Create(context), "add " + query);
    }

    /// <summary>Lists a bounded page of personal favorites with stable IDs.</summary>
    /// <param name="context">The Discord interaction.</param>
    /// <param name="page">The one-based page.</param>
    /// <returns>The shared command execution.</returns>
    [Command("list")]
    [Description("List your favorites")]
    public Task List(SlashCommandContext context, [Parameter("page")][Description("Page starting at 1")] int page = 1)
    {
        return ExecuteAsync(factory.Create(context), "list " + page.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>Deletes an owner-scoped favorite by its stable identifier.</summary>
    /// <param name="context">The Discord interaction.</param>
    /// <param name="id">The favorite identifier.</param>
    /// <returns>The shared command execution.</returns>
    [Command("remove")]
    [Description("Remove your favorite by ID")]
    public Task Remove(SlashCommandContext context, [Parameter("id")][Description("Favorite ID")] long id)
    {
        return ExecuteAsync(factory.Create(context), "remove " + id.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>Enqueues an owner-scoped favorite through normal voice validation.</summary>
    /// <param name="context">The Discord interaction.</param>
    /// <param name="id">The favorite identifier.</param>
    /// <returns>The shared command execution.</returns>
    [Command("play")]
    [Description("Play your favorite by ID")]
    public Task Play(SlashCommandContext context, [Parameter("id")][Description("Favorite ID")] long id)
    {
        return ExecuteAsync(factory.Create(context), "play " + id.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>Routes all personal favorite actions through the shared text handler.</summary>
    public Task ExecuteAsync(ISlashInteractionContext context, string argument)
    {
        return executor.ExecuteAsync(new SlashCommandExecutionRequest("favourite", context, argument,
            RequireGuild: true, Defer: true, EnsureDeferredResponse: true), default);
    }
}
