namespace DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;

/// <summary>
/// Reports the result of deleting a saved playlist.
/// </summary>
public enum DeletePlaylistResult
{

    /// <summary>
    /// The saved playlist was deleted.
    /// </summary>
    Deleted,

    /// <summary>
    /// The requested saved playlist does not exist.
    /// </summary>
    DoesNotExist,

    /// <summary>
    /// The supplied playlist name is invalid.
    /// </summary>
    InvalidPlaylistName,

    /// <summary>
    /// The operation failed for an unclassified reason.
    /// </summary>
    UnknownError
}
