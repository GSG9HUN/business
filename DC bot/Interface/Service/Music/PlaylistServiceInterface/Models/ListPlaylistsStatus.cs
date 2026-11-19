namespace DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;

/// <summary>
/// Reports the result of listing a guild's saved playlists.
/// </summary>
public enum ListPlaylistsStatus
{

    /// <summary>
    /// The saved playlists were retrieved.
    /// </summary>
    Listed,

    /// <summary>
    /// The guild has no saved playlists.
    /// </summary>
    NoPlaylists,

    /// <summary>
    /// The operation failed for an unclassified reason.
    /// </summary>
    UnknownError
}
