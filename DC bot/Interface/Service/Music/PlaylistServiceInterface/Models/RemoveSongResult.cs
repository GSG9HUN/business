namespace DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;

public enum RemoveSongResult
{
    Removed,
    PlaylistDoesNotExist,
    SongNotFound,
    InvalidPlaylistName,
    InvalidTrackNumber,
    UnknownError
}
