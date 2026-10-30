package com.dc.melodiasmario.core.network.realtime.dto

import com.dc.melodiasmario.core.model.realtime.QueueRealtimeEvent
import com.dc.melodiasmario.core.network.queue.dto.QueueDto
import kotlinx.serialization.Serializable

/**
 * Represents queue realtime event dto in the API transport contract.
 *
 * @property guildId Identifier of the guild owning this value.
 * @property eventName Event name.
 * @property updatedAtUtc Updated at utc.
 * @property snapshot Snapshot.
 */
@Serializable
data class QueueRealtimeEventDto(
    val guildId: String,
    val eventName: String,
    val updatedAtUtc: String,
    val snapshot: QueueDto
) {
    /**
     * Maps this value to domain.
     */
    fun toDomain(): QueueRealtimeEvent = QueueRealtimeEvent(
        guildId = guildId,
        eventName = eventName,
        updatedAtUtc = updatedAtUtc,
        snapshot = snapshot.toDomain()
    )
}
