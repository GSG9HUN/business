using DC_bot.Interface.Service.Music.PlaylistServiceInterface;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;

namespace DC_bot.Service.Music.PlaylistService;

/// <summary>
/// Exposes saved-playlist queries and mutations through helpers with shared limits and logging.
/// </summary>
public class PlaylistService : IPlaylistService
{
    private readonly PlaylistMutationService _mutationService;
    private readonly PlaylistQueryService _queryService;
    private readonly PlaylistTrackMutationService _trackMutationService;

    /// <summary>
    /// Composes playlist operations from the helpers registered at startup.
    /// </summary>
    /// <param name="queryService">The saved-playlist query boundary.</param>
    /// <param name="mutationService">The playlist metadata mutation boundary.</param>
    /// <param name="trackMutationService">The track loading and mutation boundary.</param>
    internal PlaylistService(PlaylistQueryService queryService, PlaylistMutationService mutationService, PlaylistTrackMutationService trackMutationService)
    {
        _queryService = queryService;
        _mutationService = mutationService;
        _trackMutationService = trackMutationService;
    }

    /// <summary>
    /// Loads source tracks into a saved playlist while enforcing names, ownership, and configured limits.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="playlistName">The playlist name supplied for this operation.</param>
    /// <param name="playlistUrl">The playlist url supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public async Task<SavePlaylistResult> SavePlaylistAsync(
        ulong guildId,
        string playlistName,
        string playlistUrl,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _trackMutationService.SavePlaylistAsync(guildId, playlistName, playlistUrl, cancellationToken);
    }

    /// <summary>
    /// Restores a saved playlist&apos;s tracks into a playback-ready load result.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="playlistName">The playlist name supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public async Task<LoadPlaylistResult> LoadPlaylistAsync(
        ulong guildId,
        string playlistName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _queryService.LoadPlaylistAsync(guildId, playlistName, cancellationToken);
    }

    /// <summary>
    /// Reads the guild&apos;s saved playlists and track counts for a list response.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public async Task<ListPlaylistsResult> ListPlaylistsAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _queryService.ListPlaylistsAsync(guildId, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Reads and restores display metadata for a named guild playlist.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="playlistName">The playlist name supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public async Task<ViewPlaylistResult> ViewPlaylistAsync(
        ulong guildId,
        string playlistName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _queryService.ViewPlaylistAsync(guildId, playlistName, cancellationToken);
    }

    /// <summary>
    /// Deletes a guild-owned playlist after validating and normalizing its name.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="playlistName">The playlist name supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public async Task<DeletePlaylistResult> DeletePlaylistAsync(
        ulong guildId,
        string playlistName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _mutationService.DeletePlaylistAsync(guildId, playlistName, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Loads a source track and appends it when playlist naming and track limits permit it.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="playlistName">The playlist name supplied for this operation.</param>
    /// <param name="songUrl">The song url supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public async Task<AddSongResult> AddSongToPlaylistAsync(
        ulong guildId,
        string playlistName,
        string songUrl,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _trackMutationService.AddSongToPlaylistAsync(guildId, playlistName, songUrl, cancellationToken);
    }

    /// <summary>
    /// Removes a numbered track from a guild-owned saved playlist.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="playlistName">The playlist name supplied for this operation.</param>
    /// <param name="trackNumber">The track number supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public async Task<RemoveSongResult> RemoveSongFromPlaylistAsync(
        ulong guildId,
        string playlistName,
        int trackNumber,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _trackMutationService.RemoveSongFromPlaylistAsync(guildId, playlistName, trackNumber, cancellationToken);
    }

    /// <summary>
    /// Renames a guild-owned playlist after validating names and checking for conflicts.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="currentName">The current name supplied for this operation.</param>
    /// <param name="newName">The new name supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public async Task<RenamePlaylistResult> RenamePlaylistAsync(
        ulong guildId,
        string currentName,
        string newName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _mutationService.RenamePlaylistAsync(guildId, currentName, newName, cancellationToken);
    }

    /// <summary>
    /// Creates a named empty playlist when its name and the guild&apos;s playlist limit permit it.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="playlistName">The playlist name supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public async Task<CreatePlaylistResult> CreatePlaylistAsync(
        ulong guildId,
        string playlistName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _mutationService.CreatePlaylistAsync(guildId, playlistName, cancellationToken);
    }
}
