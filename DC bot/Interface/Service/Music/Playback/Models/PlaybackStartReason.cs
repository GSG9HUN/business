namespace DC_bot.Interface.Service.Music.Playback.Models;

public enum PlaybackStartReason
{
    First,
    QueueNext, 
    JoinQueue, 
    RepeatCurrent, 
    RepeatList, 
    Reload, 
    Previous
}