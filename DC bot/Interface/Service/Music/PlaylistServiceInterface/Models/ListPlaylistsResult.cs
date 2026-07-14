namespace DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;

public enum ListPlaylistsStatus
{
    Listed,
    NoPlaylists,
    UnknownError
}

public sealed record PlaylistSummaryDto(
    string Name,
    int TrackCount);

public sealed record ListPlaylistsResult(
    ListPlaylistsStatus Status,
    IReadOnlyList<PlaylistSummaryDto> Playlists);
