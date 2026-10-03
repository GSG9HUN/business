package com.dc.melodiasmario.core.model.realtime

import com.dc.melodiasmario.core.model.currenttrack.PlaybackStatus

data class PlaybackRealtimeEvent(
    override val guildId: String,
    override val eventName: String,
    val updatedAtUtc: String,
    val snapshot: PlaybackStatus,
) : RealtimeEvent
