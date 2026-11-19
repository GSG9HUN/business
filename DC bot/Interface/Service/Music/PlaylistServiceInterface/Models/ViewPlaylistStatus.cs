namespace DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;

/// <summary>
/// Reports the result of retrieving a saved playlist's contents.
/// </summary>
public enum ViewPlaylistStatus
{

    /// <summary>
    /// The saved playlist contents were retrieved.
    /// </summary>
    Viewed,

    /// <summary>
    /// The requested saved playlist does not exist.
    /// </summary>
    PlaylistDoesNotExist,

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
