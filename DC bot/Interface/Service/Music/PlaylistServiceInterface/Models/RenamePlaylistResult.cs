namespace DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;

public enum RenamePlaylistResult
{
    Renamed,
    PlaylistDoesNotExist,
    PlaylistAlreadyExists,
    InvalidPlaylistName,
    UnknownError
}
