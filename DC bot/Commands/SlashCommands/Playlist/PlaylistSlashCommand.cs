using System.ComponentModel;
using DC_bot.Constants;
using DC_bot.Helper;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace DC_bot.Commands.SlashCommands.Playlist;

[Command("playlist")]
[Description("Manage saved playlists")]
public class PlaylistSlashCommand(
    ISlashCommandExecutor slashCommandExecutor,
    ISlashInteractionContextFactory contextFactory,
    ILocalizationService localizationService)
{
    [Command("create")]
    [Description("Create an empty saved playlist")]
    public Task Create(
        SlashCommandContext context,
        [Parameter("name")]
        [Description("Playlist name")]
        string name)
    {
        return ExecuteAsync("createPlaylist", contextFactory.Create(context), name.Trim());
    }

    [Command("save")]
    [Description("Save tracks from a playlist URL")]
    public Task Save(
        SlashCommandContext context,
        [Parameter("name")]
        [Description("Playlist name")]
        string name,
        [Parameter("url")]
        [Description("Playlist URL")]
        string url)
    {
        return ExecuteAsync(
            "savePlaylist",
            contextFactory.Create(context),
            $"{QuoteArgument(name)} {url.Trim()}",
            ensureDeferredResponse: true);
    }

    [Command("list")]
    [Description("List saved playlists")]
    public Task List(SlashCommandContext context)
    {
        return ExecuteAsync("listPlaylists", contextFactory.Create(context));
    }

    [Command("view")]
    [Description("View a saved playlist")]
    public Task View(
        SlashCommandContext context,
        [Parameter("name")]
        [Description("Playlist name")]
        string name)
    {
        return ExecuteAsync("viewPlaylist", contextFactory.Create(context), name.Trim());
    }

    [Command("add-song")]
    [Description("Add a song to a saved playlist")]
    public Task AddSong(
        SlashCommandContext context,
        [Parameter("name")]
        [Description("Playlist name")]
        string name,
        [Parameter("url")]
        [Description("Song URL or search query")]
        string url)
    {
        return ExecuteAsync(
            "addSong",
            contextFactory.Create(context),
            $"{QuoteArgument(name)} {url.Trim()}",
            ensureDeferredResponse: true);
    }

    [Command("remove-song")]
    [Description("Remove a song by playlist track number")]
    public Task RemoveSong(
        SlashCommandContext context,
        [Parameter("name")]
        [Description("Playlist name")]
        string name,
        [Parameter("track-number")]
        [Description("Track number from playlist view")]
        int trackNumber)
    {
        return ExecuteAsync("removeSong", contextFactory.Create(context), $"{QuoteArgument(name)} {trackNumber}");
    }

    [Command("rename")]
    [Description("Rename a saved playlist")]
    public Task Rename(
        SlashCommandContext context,
        [Parameter("current-name")]
        [Description("Current playlist name")]
        string currentName,
        [Parameter("new-name")]
        [Description("New playlist name")]
        string newName)
    {
        return ExecuteAsync(
            "renamePlaylist",
            contextFactory.Create(context),
            $"{QuoteArgument(currentName)} {QuoteArgument(newName)}");
    }

    [Command("delete")]
    [Description("Delete a saved playlist")]
    public async Task Delete(
        SlashCommandContext context,
        [Parameter("name")]
        [Description("Playlist name")]
        string name,
        [Parameter("confirm")]
        [Description("Must be true to delete the playlist")]
        bool confirm = false)
    {
        var slashContext = contextFactory.Create(context);
        var playlistName = name.Trim();
        if (!confirm)
        {
            await slashContext.RespondAsync(GetLocalizedMessage(
                slashContext,
                LocalizationKeys.DeletePlaylistCommandConfirmationRequired,
                DiscordTextSanitizer.EscapeMentions(playlistName)));
            return;
        }

        await ExecuteAsync("deletePlaylist", slashContext, playlistName);
    }

    public Task ExecuteCreateAsync(ISlashInteractionContext context, string name)
    {
        return ExecuteAsync("createPlaylist", context, name.Trim());
    }

    public Task ExecuteSaveAsync(ISlashInteractionContext context, string name, string url)
    {
        return ExecuteAsync(
            "savePlaylist",
            context,
            $"{QuoteArgument(name)} {url.Trim()}",
            ensureDeferredResponse: true);
    }

    public Task ExecuteListAsync(ISlashInteractionContext context)
    {
        return ExecuteAsync("listPlaylists", context);
    }

    public Task ExecuteViewAsync(ISlashInteractionContext context, string name)
    {
        return ExecuteAsync("viewPlaylist", context, name.Trim());
    }

    public Task ExecuteAddSongAsync(ISlashInteractionContext context, string name, string url)
    {
        return ExecuteAsync(
            "addSong",
            context,
            $"{QuoteArgument(name)} {url.Trim()}",
            ensureDeferredResponse: true);
    }

    public Task ExecuteRemoveSongAsync(ISlashInteractionContext context, string name, int trackNumber)
    {
        return ExecuteAsync("removeSong", context, $"{QuoteArgument(name)} {trackNumber}");
    }

    public Task ExecuteRenameAsync(ISlashInteractionContext context, string currentName, string newName)
    {
        return ExecuteAsync("renamePlaylist", context, $"{QuoteArgument(currentName)} {QuoteArgument(newName)}");
    }

    public async Task ExecuteDeleteAsync(ISlashInteractionContext context, string name, bool confirm)
    {
        var playlistName = name.Trim();
        if (!confirm)
        {
            await context.RespondAsync(GetLocalizedMessage(
                context,
                LocalizationKeys.DeletePlaylistCommandConfirmationRequired,
                DiscordTextSanitizer.EscapeMentions(playlistName)));
            return;
        }

        await ExecuteAsync("deletePlaylist", context, playlistName);
    }

    private Task ExecuteAsync(
        string commandName,
        ISlashInteractionContext context,
        string? argument = null,
        bool ensureDeferredResponse = false)
    {
        return slashCommandExecutor.ExecuteAsync(new SlashCommandExecutionRequest(
            commandName,
            context,
            argument,
            RequireGuild: true,
            Defer: true,
            EnsureDeferredResponse: ensureDeferredResponse));
    }

    private string GetLocalizedMessage(ISlashInteractionContext context, string key, params object[] args)
    {
        return context.GuildId is { } guildId
            ? localizationService.Get(guildId, key, args)
            : localizationService.Get(key, args);
    }

    private static string QuoteArgument(string argument)
    {
        var escaped = argument
            .Trim()
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);

        return $"\"{escaped}\"";
    }
}
