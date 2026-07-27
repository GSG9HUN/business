namespace DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;

public enum SavePlaylistResult
{
    Saved,
    AlreadyExists,
    NoTracksFound,
    InvalidPlaylistName,
    PlaylistLimitReached,
    TrackLimitExceeded,
    UnknownError
}
