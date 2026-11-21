using DSharpPlus.Entities;
using System.ComponentModel;
using DC_bot.Constants;
using DC_bot.Helper;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace DC_bot.Commands.SlashCommands.Playlist;

/// <summary>
/// Adapts the playlist slash command to shared command execution and interaction acknowledgement.
/// </summary>
/// <param name="slashCommandExecutor">The slash command executor dependency used by this boundary.</param>
/// <param name="contextFactory">The slash interaction context factory dependency used by this boundary.</param>
/// <param name="localizationService">The localization service dependency used by this boundary.</param>
/// <param name="responseBuilder">Creates localized embeds through the shared presentation boundary.</param>
[Command("playlist")]
[Description("Manage saved playlists")]
public class PlaylistSlashCommand(
    ISlashCommandExecutor slashCommandExecutor,
    ISlashInteractionContextFactory contextFactory,
    ILocalizationService localizationService,
    IResponseBuilder responseBuilder)
{

    /// <summary>
    /// Adapts the create slash route to its shared handler and interaction response policy.
    /// </summary>
    /// <param name="context">The context supplied for this operation.</param>
    /// <param name="name">The name supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    [Command("create")]
    [Description("Create an empty saved playlist")]
    public Task Create(SlashCommandContext context, [Parameter("name")][Description("Playlist name")] string name)
    {
        return ExecuteAsync(CommandNames.CreatePlaylist, contextFactory.Create(context), name.Trim());
    }

    /// <summary>
    /// Adapts the save slash route to its shared handler and interaction response policy.
    /// </summary>
    /// <param name="context">The context supplied for this operation.</param>
    /// <param name="name">The name supplied for this operation.</param>
    /// <param name="url">The url supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    [Command("import")]
    [Description("Import tracks from a URL or search query into a new saved playlist")]
    public Task Import(
        SlashCommandContext context,
        [Parameter("name")][Description("Playlist name")] string name,
        [Parameter("url")][Description("Source URL or search query")] string url)
    {
        return ExecuteAsync(CommandNames.ImportPlaylist, contextFactory.Create(context), $"{QuoteArgument(name)} {url.Trim()}", ensureDeferredResponse: true);
    }

    /// <summary>
    /// Adapts the list slash route to its shared handler and interaction response policy.
    /// </summary>
    /// <param name="context">The context supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    [Command("list")]
    [Description("List saved playlists")]
    public Task List(SlashCommandContext context)
    {
        return ExecuteAsync(CommandNames.ListPlaylists, contextFactory.Create(context));
    }

    /// <summary>
    /// Adapts the view slash route to its shared handler and interaction response policy.
    /// </summary>
    /// <param name="context">The context supplied for this operation.</param>
    /// <param name="name">The name supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    [Command("view")]
    [Description("View a saved playlist")]
    public Task View(SlashCommandContext context, [Parameter("name")][Description("Playlist name")] string name)
    {
        return ExecuteAsync(CommandNames.ViewPlaylist, contextFactory.Create(context), name.Trim());
    }

    /// <summary>
    /// Adapts the load slash route to its shared handler and interaction response policy.
    /// </summary>
    /// <param name="context">The context supplied for this operation.</param>
    /// <param name="name">The name supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    [Command("load")]
    [Description("Load a saved playlist into the queue")]
    public Task Load(SlashCommandContext context, [Parameter("name")][Description("Playlist name")] string name)
    {
        return ExecuteAsync(CommandNames.LoadPlaylist, contextFactory.Create(context), name.Trim(), ensureDeferredResponse: true);
    }

    /// <summary>
    /// Adapts the add song slash route to its shared handler and interaction response policy.
    /// </summary>
    /// <param name="context">The context supplied for this operation.</param>
    /// <param name="name">The name supplied for this operation.</param>
    /// <param name="url">The url supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    [Command("add-song")]
    [Description("Add a song to a saved playlist")]
    public Task AddSong(
        SlashCommandContext context,
        [Parameter("name")][Description("Playlist name")] string name,
        [Parameter("url")][Description("Song URL or search query")] string url)
    {
        return ExecuteAsync(CommandNames.AddSongToPlaylist, contextFactory.Create(context), $"{QuoteArgument(name)} {url.Trim()}", ensureDeferredResponse: true);
    }

    /// <summary>
    /// Adapts the remove song slash route to its shared handler and interaction response policy.
    /// </summary>
    /// <param name="context">The context supplied for this operation.</param>
    /// <param name="name">The name supplied for this operation.</param>
    /// <param name="trackNumber">The track number supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    [Command("remove-song")]
    [Description("Remove a song by playlist track number")]
    public Task RemoveSong(
        SlashCommandContext context,
        [Parameter("name")][Description("Playlist name")] string name,
        [Parameter("track-number")][Description("Track number from playlist view")] int trackNumber)
    {
        return ExecuteAsync(CommandNames.RemoveSongFromPlaylist, contextFactory.Create(context), $"{QuoteArgument(name)} {trackNumber}");
    }

    /// <summary>
    /// Adapts the rename slash route to its shared handler and interaction response policy.
    /// </summary>
    /// <param name="context">The context supplied for this operation.</param>
    /// <param name="currentName">The current name supplied for this operation.</param>
    /// <param name="newName">The new name supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    [Command("rename")]
    [Description("Rename a saved playlist")]
    public Task Rename(
        SlashCommandContext context,
        [Parameter("current-name")][Description("Current playlist name")] string currentName,
        [Parameter("new-name")][Description("New playlist name")] string newName)
    {
        return ExecuteAsync(CommandNames.RenamePlaylist, contextFactory.Create(context), $"{QuoteArgument(currentName)} {QuoteArgument(newName)}");
    }

    /// <summary>
    /// Adapts the delete slash route to its shared handler and interaction response policy.
    /// </summary>
    /// <param name="context">The context supplied for this operation.</param>
    /// <param name="name">The name supplied for this operation.</param>
    /// <param name="confirm">The confirm supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    [Command("delete")]
    [Description("Delete a saved playlist")]
    public async Task Delete(
        SlashCommandContext context,
        [Parameter("name")][Description("Playlist name")] string name,
        [Parameter("confirm")][Description("Must be true to delete the playlist")] bool confirm = false)
    {
        var slashContext = contextFactory.Create(context);
        var playlistName = name.Trim();

        if (!confirm)
        {
            await slashContext.RespondAsync(responseBuilder.CreateEmbed(slashContext.GuildId,
                GetLocalizedMessage(slashContext, LocalizationKeys.DeletePlaylistCommandConfirmationRequired,
                    DiscordTextSanitizer.EscapeMentions(playlistName)), DiscordColor.Gold, LocalizationKeys.ResponseWarningTitle));
            return;
        }

        await ExecuteAsync(CommandNames.DeletePlaylist, slashContext, playlistName);
    }

    /// <summary>
    /// Routes the slash interaction through the shared command executor and acknowledgement policy.
    /// </summary>
    /// <param name="commandName">The command name supplied for this operation.</param>
    /// <param name="context">The slash interaction context dependency used by this boundary.</param>
    /// <param name="argument">The argument supplied for this operation.</param>
    /// <param name="ensureDeferredResponse">The ensure deferred response supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    private Task ExecuteAsync(
        string commandName,
        ISlashInteractionContext context,
        string? argument = null,
        bool ensureDeferredResponse = false)
    {
        return slashCommandExecutor.ExecuteAsync(new SlashCommandExecutionRequest(commandName, context, argument, RequireGuild: true, Defer: true, EnsureDeferredResponse: ensureDeferredResponse), default);
    }

    /// <summary>
    /// Resolves a response template and applies formatting arguments.
    /// </summary>
    /// <param name="context">The slash interaction context dependency used by this boundary.</param>
    /// <param name="key">The key supplied for this operation.</param>
    /// <param name="args">The args supplied for this operation.</param>
    /// <returns>The formatted, serialized, or resolved text value.</returns>
    private string GetLocalizedMessage(ISlashInteractionContext context, string key, params object[] args)
    {
        return context.GuildId is { } guildId ? localizationService.Get(guildId, key, args) : localizationService.Get(key, args);
    }

    /// <summary>
    /// Quotes an argument so parsing preserves it as a single value.
    /// </summary>
    /// <param name="argument">The argument supplied for this operation.</param>
    /// <returns>The formatted, serialized, or resolved text value.</returns>
    private static string QuoteArgument(string argument)
    {
        var escaped = argument.Trim()
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);
        return $"\"{escaped}\"";
    }
}
