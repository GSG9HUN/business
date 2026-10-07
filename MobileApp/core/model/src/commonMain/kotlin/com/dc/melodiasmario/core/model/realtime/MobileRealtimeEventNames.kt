package com.dc.melodiasmario.core.model.realtime

object MobileRealtimeEventNames {
    const val BotControlCommandCreated = "BotControlCommandCreated"
    const val BotControlCommandStarted = "BotControlCommandStarted"
    const val BotControlCommandSucceeded = "BotControlCommandSucceeded"
    const val BotControlCommandFailed = "BotControlCommandFailed"
    const val BotControlCommandInterrupted = "BotControlCommandInterrupted"
    const val BotControlCommandUpdated = "BotControlCommandUpdated"

    const val PlaybackSnapshotChanged = "PlaybackSnapshotChanged"
    const val CurrentTrackChanged = "CurrentTrackChanged"
    const val PlaybackStarted = "PlaybackStarted"
    const val PlaybackPaused = "PlaybackPaused"
    const val PlaybackResumed = "PlaybackResumed"
    const val PlaybackStopped = "PlaybackStopped"
    const val PlaybackSkipped = "PlaybackSkipped"
    const val PlaybackPreviousStarted = "PlaybackPreviousStarted"
    const val RepeatModeChanged = "RepeatModeChanged"
    const val PlaybackPositionChanged = "PlaybackPositionChanged"
    const val PlaybackLoadFailed = "PlaybackLoadFailed"

    const val QueueSnapshotChanged = "QueueSnapshotChanged"
    const val QueueItemAdded = "QueueItemAdded"
    const val QueueItemsAdded = "QueueItemsAdded"
    const val QueueItemRemoved = "QueueItemRemoved"
    const val QueueCleared = "QueueCleared"
    const val QueueShuffled = "QueueShuffled"
    const val QueueItemMoved = "QueueItemMoved"
    const val QueueItemClaimed = "QueueItemClaimed"
    const val QueueCompacted = "QueueCompacted"
    const val RepeatListSnapshotChanged = "RepeatListSnapshotChanged"

    const val GuildBotStatusChanged = "GuildBotStatusChanged"
    const val BotJoinedVoiceChannel = "BotJoinedVoiceChannel"
    const val BotLeftVoiceChannel = "BotLeftVoiceChannel"
    const val BotVoiceUserCountChanged = "BotVoiceUserCountChanged"
    const val GuildAccessChanged = "GuildAccessChanged"
    const val GuildListChanged = "GuildListChanged"

    const val ProfileUpdated = "ProfileUpdated"
    const val SessionRevoked = "SessionRevoked"
    const val SessionRefreshRequired = "SessionRefreshRequired"
}
