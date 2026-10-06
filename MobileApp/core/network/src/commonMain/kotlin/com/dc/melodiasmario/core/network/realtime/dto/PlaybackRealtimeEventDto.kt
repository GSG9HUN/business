package com.dc.melodiasmario.core.network.realtime.dto

import com.dc.melodiasmario.core.model.realtime.PlaybackRealtimeEvent
import com.dc.melodiasmario.core.network.currenttrack.dto.PlaybackStatusDto
import kotlinx.serialization.Serializable

@Serializable
data class PlaybackRealtimeEventDto(
    val guildId: String,
    val eventName: String,
    val updatedAtUtc: String,
    val snapshot: PlaybackStatusDto
) {
    fun toDomain() = PlaybackRealtimeEvent(
        guildId = guildId,
        eventName = eventName,
        updatedAtUtc = updatedAtUtc,
        snapshot = snapshot.toDomain()
    )
}