namespace DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;

public enum LoadPlaylistStatus
{
    Loaded,
    NotFound,
    EmptyPlaylist,
    InvalidPlaylistName,
    UnknownError
}

public sealed record LoadPlaylistResult(
    LoadPlaylistStatus Status,
    PlaylistDto? Playlist);
