namespace DC_bot.BotControl;

/// <summary>
/// Defines stable realtime event names shared by persistence, API, bot, and mobile consumers.
/// </summary>
public static class MobileRealtimeEventNames
{

    /// <summary>
    /// The realtime protocol event name for bot control command created notifications.
    /// </summary>
    public const string BotControlCommandCreated = "BotControlCommandCreated";

    /// <summary>
    /// The realtime protocol event name for bot control command started notifications.
    /// </summary>
    public const string BotControlCommandStarted = "BotControlCommandStarted";

    /// <summary>
    /// The realtime protocol event name for bot control command succeeded notifications.
    /// </summary>
    public const string BotControlCommandSucceeded = "BotControlCommandSucceeded";

    /// <summary>
    /// The realtime protocol event name for bot control command failed notifications.
    /// </summary>
    public const string BotControlCommandFailed = "BotControlCommandFailed";

    /// <summary>
    /// The realtime protocol event name for bot control command interrupted notifications.
    /// </summary>
    public const string BotControlCommandInterrupted = "BotControlCommandInterrupted";

    /// <summary>
    /// The realtime protocol event name for bot control command updated notifications.
    /// </summary>
    public const string BotControlCommandUpdated = "BotControlCommandUpdated";

    /// <summary>
    /// The realtime protocol event name for playback snapshot changed notifications.
    /// </summary>
    public const string PlaybackSnapshotChanged = "PlaybackSnapshotChanged";

    /// <summary>
    /// The realtime protocol event name for current track changed notifications.
    /// </summary>
    public const string CurrentTrackChanged = "CurrentTrackChanged";

    /// <summary>
    /// The realtime protocol event name for playback started notifications.
    /// </summary>
    public const string PlaybackStarted = "PlaybackStarted";

    /// <summary>
    /// The realtime protocol event name for playback paused notifications.
    /// </summary>
    public const string PlaybackPaused = "PlaybackPaused";

    /// <summary>
    /// The realtime protocol event name for playback resumed notifications.
    /// </summary>
    public const string PlaybackResumed = "PlaybackResumed";

    /// <summary>
    /// The realtime protocol event name for playback stopped notifications.
    /// </summary>
    public const string PlaybackStopped = "PlaybackStopped";

    /// <summary>
    /// The realtime protocol event name for playback skipped notifications.
    /// </summary>
    public const string PlaybackSkipped = "PlaybackSkipped";

    /// <summary>
    /// The realtime protocol event name for playback previous started notifications.
    /// </summary>
    public const string PlaybackPreviousStarted = "PlaybackPreviousStarted";

    /// <summary>
    /// The realtime protocol event name for repeat mode changed notifications.
    /// </summary>
    public const string RepeatModeChanged = "RepeatModeChanged";

    /// <summary>
    /// The realtime protocol event name for playback position changed notifications.
    /// </summary>
    public const string PlaybackPositionChanged = "PlaybackPositionChanged";

    /// <summary>
    /// The realtime protocol event name for playback load failed notifications.
    /// </summary>
    public const string PlaybackLoadFailed = "PlaybackLoadFailed";

    /// <summary>
    /// The realtime protocol event name for queue snapshot changed notifications.
    /// </summary>
    public const string QueueSnapshotChanged = "QueueSnapshotChanged";

    /// <summary>
    /// The realtime protocol event name for queue item added notifications.
    /// </summary>
    public const string QueueItemAdded = "QueueItemAdded";

    /// <summary>
    /// The realtime protocol event name for queue items added notifications.
    /// </summary>
    public const string QueueItemsAdded = "QueueItemsAdded";

    /// <summary>
    /// The realtime protocol event name for queue item removed notifications.
    /// </summary>
    public const string QueueItemRemoved = "QueueItemRemoved";

    /// <summary>
    /// The realtime protocol event name for queue cleared notifications.
    /// </summary>
    public const string QueueCleared = "QueueCleared";

    /// <summary>
    /// The realtime protocol event name for queue shuffled notifications.
    /// </summary>
    public const string QueueShuffled = "QueueShuffled";

    /// <summary>
    /// The realtime protocol event name for queue item moved notifications.
    /// </summary>
    public const string QueueItemMoved = "QueueItemMoved";

    /// <summary>
    /// The realtime protocol event name for queue item claimed notifications.
    /// </summary>
    public const string QueueItemClaimed = "QueueItemClaimed";

    /// <summary>
    /// The realtime protocol event name for queue compacted notifications.
    /// </summary>
    public const string QueueCompacted = "QueueCompacted";

    /// <summary>
    /// The realtime protocol event name for repeat list snapshot changed notifications.
    /// </summary>
    public const string RepeatListSnapshotChanged = "RepeatListSnapshotChanged";

    /// <summary>
    /// The realtime protocol event name for guild bot status changed notifications.
    /// </summary>
    public const string GuildBotStatusChanged = "GuildBotStatusChanged";

    /// <summary>
    /// The realtime protocol event name for bot joined voice channel notifications.
    /// </summary>
    public const string BotJoinedVoiceChannel = "BotJoinedVoiceChannel";

    /// <summary>
    /// The realtime protocol event name for bot left voice channel notifications.
    /// </summary>
    public const string BotLeftVoiceChannel = "BotLeftVoiceChannel";

    /// <summary>
    /// The realtime protocol event name for bot voice user count changed notifications.
    /// </summary>
    public const string BotVoiceUserCountChanged = "BotVoiceUserCountChanged";

    /// <summary>
    /// The realtime protocol event name for guild access changed notifications.
    /// </summary>
    public const string GuildAccessChanged = "GuildAccessChanged";

    /// <summary>
    /// The realtime protocol event name for guild list changed notifications.
    /// </summary>
    public const string GuildListChanged = "GuildListChanged";

    /// <summary>
    /// The realtime protocol event name for profile updated notifications.
    /// </summary>
    public const string ProfileUpdated = "ProfileUpdated";

    /// <summary>
    /// The realtime protocol event name for session revoked notifications.
    /// </summary>
    public const string SessionRevoked = "SessionRevoked";

    /// <summary>
    /// The realtime protocol event name for session refresh required notifications.
    /// </summary>
    public const string SessionRefreshRequired = "SessionRefreshRequired";
}
