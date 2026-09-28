namespace DC_bot.BotControl;

public static class MobileRealtimeEventNames
{
    public const string BotControlCommandCreated = "BotControlCommandCreated";
    public const string BotControlCommandStarted = "BotControlCommandStarted";
    public const string BotControlCommandSucceeded = "BotControlCommandSucceeded";
    public const string BotControlCommandFailed = "BotControlCommandFailed";
    public const string BotControlCommandInterrupted = "BotControlCommandInterrupted";
    public const string BotControlCommandUpdated = "BotControlCommandUpdated";

    public const string PlaybackSnapshotChanged = "PlaybackSnapshotChanged";
    public const string CurrentTrackChanged = "CurrentTrackChanged";
    public const string PlaybackStarted = "PlaybackStarted";
    public const string PlaybackPaused = "PlaybackPaused";
    public const string PlaybackResumed = "PlaybackResumed";
    public const string PlaybackStopped = "PlaybackStopped";
    public const string PlaybackSkipped = "PlaybackSkipped";
    public const string PlaybackPreviousStarted = "PlaybackPreviousStarted";
    public const string RepeatModeChanged = "RepeatModeChanged";
    public const string PlaybackPositionChanged = "PlaybackPositionChanged";
    public const string PlaybackLoadFailed = "PlaybackLoadFailed";

    public const string QueueSnapshotChanged = "QueueSnapshotChanged";
    public const string QueueItemAdded = "QueueItemAdded";
    public const string QueueItemsAdded = "QueueItemsAdded";
    public const string QueueItemRemoved = "QueueItemRemoved";
    public const string QueueCleared = "QueueCleared";
    public const string QueueShuffled = "QueueShuffled";
    public const string QueueItemMoved = "QueueItemMoved";
    public const string QueueItemClaimed = "QueueItemClaimed";
    public const string QueueCompacted = "QueueCompacted";
    public const string RepeatListSnapshotChanged = "RepeatListSnapshotChanged";

    public const string GuildBotStatusChanged = "GuildBotStatusChanged";
    public const string BotJoinedVoiceChannel = "BotJoinedVoiceChannel";
    public const string BotLeftVoiceChannel = "BotLeftVoiceChannel";
    public const string BotVoiceUserCountChanged = "BotVoiceUserCountChanged";
    public const string GuildAccessChanged = "GuildAccessChanged";
    public const string GuildListChanged = "GuildListChanged";

    public const string ProfileUpdated = "ProfileUpdated";
    public const string SessionRevoked = "SessionRevoked";
    public const string SessionRefreshRequired = "SessionRefreshRequired";
}
