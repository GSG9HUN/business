namespace DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;

public enum AddSongResult
{
    Added,
    PlaylistDoesNotExist,
    NoTracksFound,
    InvalidSongUrl,
    InvalidPlaylistName,
    TrackLimitReached,
    UnknownError
}
