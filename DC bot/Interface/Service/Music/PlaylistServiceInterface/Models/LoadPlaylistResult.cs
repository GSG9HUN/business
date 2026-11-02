namespace DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;

/// <summary>
/// Reports the result of loading a saved playlist into the queue.
/// </summary>
public enum LoadPlaylistStatus
{

    /// <summary>
    /// The saved playlist was loaded into the playback queue.
    /// </summary>
    Loaded,

    /// <summary>
    /// The requested resource was not found.
    /// </summary>
    NotFound,

    /// <summary>
    /// The saved playlist contains no tracks.
    /// </summary>
    EmptyPlaylist,

    /// <summary>
    /// The supplied playlist name is invalid.
    /// </summary>
    InvalidPlaylistName,

    /// <summary>
    /// The operation failed for an unclassified reason.
    /// </summary>
    UnknownError
}

/// <summary>
/// Carries outcome and response data for load playlist operations.
/// </summary>
/// <param name="Status">The status supplied for this operation.</param>
/// <param name="Playlist">The playlist supplied for this operation.</param>
public sealed record LoadPlaylistResult(LoadPlaylistStatus Status, PlaylistDto? Playlist);
