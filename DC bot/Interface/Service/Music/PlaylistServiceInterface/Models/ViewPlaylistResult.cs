namespace DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;

public enum ViewPlaylistStatus
{
    Viewed,
    PlaylistDoesNotExist,
    EmptyPlaylist,
    UnknownError
}

public sealed record PlaylistViewTrackDto(
    int OrderNumber,
    string Title,
    string Author,
    TimeSpan Duration,
    string TrackUri);

public sealed record ViewPlaylistResult(
    ViewPlaylistStatus Status,
    string PlaylistName,
    IReadOnlyList<PlaylistViewTrackDto> Tracks);
