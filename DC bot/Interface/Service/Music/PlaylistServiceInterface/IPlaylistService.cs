using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;

namespace DC_bot.Interface.Service.Music.PlaylistServiceInterface;

/// <summary>
/// Defines the playlist service boundary so callers are independent of its concrete implementation.
/// </summary>
public interface IPlaylistService
{

    /// <summary>
    /// Loads source tracks into a saved playlist while enforcing names, ownership, and configured limits.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="playlistName">The playlist name supplied for this operation.</param>
    /// <param name="playlistUrl">The playlist url supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    Task<SavePlaylistResult> SavePlaylistAsync(
        ulong guildId,
        string playlistName,
        string playlistUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Restores a saved playlist&apos;s tracks into a playback-ready load result.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="playlistName">The playlist name supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    Task<LoadPlaylistResult> LoadPlaylistAsync(ulong guildId, string playlistName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads the guild&apos;s saved playlists and track counts for a list response.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    Task<ListPlaylistsResult> ListPlaylistsAsync(ulong guildId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads and restores display metadata for a named guild playlist.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="playlistName">The playlist name supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    Task<ViewPlaylistResult> ViewPlaylistAsync(ulong guildId, string playlistName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a guild-owned playlist after validating and normalizing its name.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="playlistName">The playlist name supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    Task<DeletePlaylistResult> DeletePlaylistAsync(ulong guildId, string playlistName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a source track and appends it when playlist naming and track limits permit it.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="playlistName">The playlist name supplied for this operation.</param>
    /// <param name="songUrl">The song url supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    Task<AddSongResult> AddSongToPlaylistAsync(
        ulong guildId,
        string playlistName,
        string songUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a numbered track from a guild-owned saved playlist.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="playlistName">The playlist name supplied for this operation.</param>
    /// <param name="trackNumber">The track number supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    Task<RemoveSongResult> RemoveSongFromPlaylistAsync(
        ulong guildId,
        string playlistName,
        int trackNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Renames a guild-owned playlist after validating names and checking for conflicts.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="currentName">The current name supplied for this operation.</param>
    /// <param name="newName">The new name supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    Task<RenamePlaylistResult> RenamePlaylistAsync(
        ulong guildId,
        string currentName,
        string newName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a named empty playlist when its name and the guild&apos;s playlist limit permit it.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="playlistName">The playlist name supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    Task<CreatePlaylistResult> CreatePlaylistAsync(ulong guildId, string playlistName, CancellationToken cancellationToken = default);
}
