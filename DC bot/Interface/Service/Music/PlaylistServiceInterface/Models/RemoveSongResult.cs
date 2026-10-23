namespace DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;

/// <summary>
/// Reports the result of removing a song from a saved playlist.
/// </summary>
public enum RemoveSongResult
{

    /// <summary>
    /// The requested track was removed from the saved playlist.
    /// </summary>
    Removed,

    /// <summary>
    /// The requested saved playlist does not exist.
    /// </summary>
    PlaylistDoesNotExist,

    /// <summary>
    /// The requested song was not found in the saved playlist.
    /// </summary>
    SongNotFound,

    /// <summary>
    /// The supplied playlist name is invalid.
    /// </summary>
    InvalidPlaylistName,

    /// <summary>
    /// The supplied playlist track index is invalid.
    /// </summary>
    InvalidTrackNumber,

    /// <summary>
    /// The operation failed for an unclassified reason.
    /// </summary>
    UnknownError
}
