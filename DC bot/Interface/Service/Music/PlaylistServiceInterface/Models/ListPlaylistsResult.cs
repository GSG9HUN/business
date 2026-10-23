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

/// <summary>
/// Carries outcome and response data for list playlists operations.
/// </summary>
/// <param name="Status">The status supplied for this operation.</param>
/// <param name="Playlists">The playlists supplied for this operation.</param>
public sealed record ListPlaylistsResult(ListPlaylistsStatus Status, IReadOnlyList<PlaylistSummaryDto> Playlists);
