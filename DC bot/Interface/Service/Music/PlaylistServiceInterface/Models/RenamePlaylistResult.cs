namespace DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;

/// <summary>
/// Reports the result of renaming a saved playlist.
/// </summary>
public enum RenamePlaylistResult
{

    /// <summary>
    /// The saved playlist was renamed.
    /// </summary>
    Renamed,

    /// <summary>
    /// The requested saved playlist does not exist.
    /// </summary>
    PlaylistDoesNotExist,

    /// <summary>
    /// A playlist with the requested name already exists.
    /// </summary>
    PlaylistAlreadyExists,

    /// <summary>
    /// The supplied playlist name is invalid.
    /// </summary>
    InvalidPlaylistName,

    /// <summary>
    /// The operation failed for an unclassified reason.
    /// </summary>
    UnknownError
}
