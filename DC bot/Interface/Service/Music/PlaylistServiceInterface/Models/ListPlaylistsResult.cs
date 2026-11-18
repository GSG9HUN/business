namespace DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;

/// <summary>
/// Carries outcome and response data for list playlists operations.
/// </summary>
/// <param name="Status">The status supplied for this operation.</param>
/// <param name="Playlists">The playlists supplied for this operation.</param>
public sealed record ListPlaylistsResult(ListPlaylistsStatus Status, IReadOnlyList<PlaylistSummaryDto> Playlists);
