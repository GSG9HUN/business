namespace DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;

public enum CreatePlaylistResult
{
    Created,
    PlaylistAlreadyExists,
    InvalidPlaylistName,
    PlaylistLimitReached,
    UnknownError
}
