namespace DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;

/// <summary>
/// Reports the result of adding a song to a saved playlist.
/// </summary>
public enum AddSongResult
{

    /// <summary>
    /// The song was added to the saved playlist.
    /// </summary>
    Added,

    /// <summary>
    /// The requested saved playlist does not exist.
    /// </summary>
    PlaylistDoesNotExist,

    /// <summary>
    /// No playable tracks were resolved from the supplied input.
    /// </summary>
    NoTracksFound,

    /// <summary>
    /// The supplied song URL is invalid.
    /// </summary>
    InvalidSongUrl,

    /// <summary>
    /// The supplied playlist name is invalid.
    /// </summary>
    InvalidPlaylistName,

    /// <summary>
    /// The playlist cannot accept more tracks because its track limit was reached.
    /// </summary>
    TrackLimitReached,

    /// <summary>
    /// The operation failed for an unclassified reason.
    /// </summary>
    UnknownError
}
