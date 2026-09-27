package com.dc.melodiasmario.core.network.realtime.dto

import com.dc.melodiasmario.core.model.realtime.QueueRealtimeEvent
import com.dc.melodiasmario.core.network.queue.dto.QueueDto
import kotlinx.serialization.Serializable

@Serializable
data class QueueRealtimeEventDto(
    val guildId: String,
    val eventName: String,
    val updatedAtUtc: String,
    val snapshot: QueueDto
) {
    fun toDomain() = QueueRealtimeEvent(
        guildId = guildId,
        eventName = eventName,
        updatedAtUtc = updatedAtUtc,
        snapshot = snapshot.toDomain()
    )
}