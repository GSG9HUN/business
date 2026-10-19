namespace DC_bot.Interface.Service.Music.Playback.Models;

public enum PlaybackStartStatus
{
    Empty, 
    Queued, 
    Started, 
    NotStarted, 
    StartedStatePending,
    Unknown, 
    Superseded, 
    Busy
}