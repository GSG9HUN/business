namespace DC_bot.Service.Music.Lifecycle;

public sealed class PlaybackLifecycleOptions
{
    public TimeSpan JoinDeadline { get; set; }
    public TimeSpan LeaveDeadline { get; set; }
}