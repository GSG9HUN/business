namespace DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;

/// <summary>
/// Reports the result of creating a saved playlist.
/// </summary>
public enum CreatePlaylistResult
{

    /// <summary>
    /// The playlist was created.
    /// </summary>
    Created,

    /// <summary>
    /// A playlist with the requested name already exists.
    /// </summary>
    PlaylistAlreadyExists,

    /// <summary>
    /// The supplied playlist name is invalid.
    /// </summary>
    InvalidPlaylistName,

    /// <summary>
    /// The guild cannot create another playlist because its playlist limit was reached.
    /// </summary>
    PlaylistLimitReached,

    /// <summary>
    /// The operation failed for an unclassified reason.
    /// </summary>
    UnknownError
}
