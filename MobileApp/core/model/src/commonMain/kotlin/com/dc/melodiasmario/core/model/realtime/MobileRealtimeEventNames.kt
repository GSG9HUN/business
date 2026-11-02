package com.dc.melodiasmario.core.model.realtime

/**
 * Represents mobile realtime event names shared between application layers.
 */
object MobileRealtimeEventNames {
    /**
     * Bot control command created exposed by mobile realtime event names.
     */
    const val BotControlCommandCreated: String = "BotControlCommandCreated"

    /**
     * Bot control command started exposed by mobile realtime event names.
     */
    const val BotControlCommandStarted: String = "BotControlCommandStarted"

    /**
     * Bot control command succeeded exposed by mobile realtime event names.
     */
    const val BotControlCommandSucceeded: String = "BotControlCommandSucceeded"

    /**
     * Bot control command failed exposed by mobile realtime event names.
     */
    const val BotControlCommandFailed: String = "BotControlCommandFailed"

    /**
     * Bot control command interrupted exposed by mobile realtime event names.
     */
    const val BotControlCommandInterrupted: String = "BotControlCommandInterrupted"

    /**
     * Bot control command updated exposed by mobile realtime event names.
     */
    const val BotControlCommandUpdated: String = "BotControlCommandUpdated"

    /**
     * Playback snapshot changed exposed by mobile realtime event names.
     */
    const val PlaybackSnapshotChanged: String = "PlaybackSnapshotChanged"

    /**
     * Current track changed exposed by mobile realtime event names.
     */
    const val CurrentTrackChanged: String = "CurrentTrackChanged"

    /**
     * Playback started exposed by mobile realtime event names.
     */
    const val PlaybackStarted: String = "PlaybackStarted"

    /**
     * Playback paused exposed by mobile realtime event names.
     */
    const val PlaybackPaused: String = "PlaybackPaused"

    /**
     * Playback resumed exposed by mobile realtime event names.
     */
    const val PlaybackResumed: String = "PlaybackResumed"

    /**
     * Playback stopped exposed by mobile realtime event names.
     */
    const val PlaybackStopped: String = "PlaybackStopped"

    /**
     * Playback skipped exposed by mobile realtime event names.
     */
    const val PlaybackSkipped: String = "PlaybackSkipped"

    /**
     * Playback previous started exposed by mobile realtime event names.
     */
    const val PlaybackPreviousStarted: String = "PlaybackPreviousStarted"

    /**
     * Repeat mode changed exposed by mobile realtime event names.
     */
    const val RepeatModeChanged: String = "RepeatModeChanged"

    /**
     * Playback position changed exposed by mobile realtime event names.
     */
    const val PlaybackPositionChanged: String = "PlaybackPositionChanged"

    /**
     * Playback load failed exposed by mobile realtime event names.
     */
    const val PlaybackLoadFailed: String = "PlaybackLoadFailed"

    /**
     * Queue snapshot changed exposed by mobile realtime event names.
     */
    const val QueueSnapshotChanged: String = "QueueSnapshotChanged"

    /**
     * Queue item added exposed by mobile realtime event names.
     */
    const val QueueItemAdded: String = "QueueItemAdded"

    /**
     * Queue items added exposed by mobile realtime event names.
     */
    const val QueueItemsAdded: String = "QueueItemsAdded"

    /**
     * Queue item removed exposed by mobile realtime event names.
     */
    const val QueueItemRemoved: String = "QueueItemRemoved"

    /**
     * Queue cleared exposed by mobile realtime event names.
     */
    const val QueueCleared: String = "QueueCleared"

    /**
     * Queue shuffled exposed by mobile realtime event names.
     */
    const val QueueShuffled: String = "QueueShuffled"

    /**
     * Queue item moved exposed by mobile realtime event names.
     */
    const val QueueItemMoved: String = "QueueItemMoved"

    /**
     * Queue item claimed exposed by mobile realtime event names.
     */
    const val QueueItemClaimed: String = "QueueItemClaimed"

    /**
     * Queue compacted exposed by mobile realtime event names.
     */
    const val QueueCompacted: String = "QueueCompacted"

    /**
     * Repeat list snapshot changed exposed by mobile realtime event names.
     */
    const val RepeatListSnapshotChanged: String = "RepeatListSnapshotChanged"

    /**
     * Guild bot status changed exposed by mobile realtime event names.
     */
    const val GuildBotStatusChanged: String = "GuildBotStatusChanged"

    /**
     * Bot joined voice channel exposed by mobile realtime event names.
     */
    const val BotJoinedVoiceChannel: String = "BotJoinedVoiceChannel"

    /**
     * Bot left voice channel exposed by mobile realtime event names.
     */
    const val BotLeftVoiceChannel: String = "BotLeftVoiceChannel"

    /**
     * Bot voice user count changed exposed by mobile realtime event names.
     */
    const val BotVoiceUserCountChanged: String = "BotVoiceUserCountChanged"
}
