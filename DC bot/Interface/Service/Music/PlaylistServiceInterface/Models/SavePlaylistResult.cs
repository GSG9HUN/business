namespace DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;

/// <summary>
/// Reports the result of saving tracks as a playlist.
/// </summary>
public enum SavePlaylistResult
{

    /// <summary>
    /// The playlist was saved.
    /// </summary>
    Saved,

    /// <summary>
    /// A saved playlist with this name already exists.
    /// </summary>
    AlreadyExists,

    /// <summary>
    /// No playable tracks were resolved from the supplied input.
    /// </summary>
    NoTracksFound,

    /// <summary>
    /// The supplied playlist name is invalid.
    /// </summary>
    InvalidPlaylistName,

    /// <summary>
    /// The guild cannot create another playlist because its playlist limit was reached.
    /// </summary>
    PlaylistLimitReached,

    /// <summary>
    /// The supplied tracks exceed the saved playlist track limit.
    /// </summary>
    TrackLimitExceeded,

    /// <summary>
    /// The operation failed for an unclassified reason.
    /// </summary>
    UnknownError
}
