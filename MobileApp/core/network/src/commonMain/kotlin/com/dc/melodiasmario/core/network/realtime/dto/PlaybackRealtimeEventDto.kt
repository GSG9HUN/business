package com.dc.melodiasmario.core.network.realtime.dto

import com.dc.melodiasmario.core.model.realtime.PlaybackRealtimeEvent
import com.dc.melodiasmario.core.network.currenttrack.dto.PlaybackStatusDto
import kotlinx.serialization.Serializable

/**
 * Represents playback realtime event dto in the API transport contract.
 *
 * @property guildId Identifier of the guild owning this value.
 * @property eventName Event name.
 * @property updatedAtUtc Updated at utc.
 * @property snapshot Snapshot.
 */
@Serializable
data class PlaybackRealtimeEventDto(
    val guildId: String,
    val eventName: String,
    val updatedAtUtc: String,
    val snapshot: PlaybackStatusDto
) {
    /**
     * Maps this value to domain.
     */
    fun toDomain(): PlaybackRealtimeEvent = PlaybackRealtimeEvent(
        guildId = guildId,
        eventName = eventName,
        updatedAtUtc = updatedAtUtc,
        snapshot = snapshot.toDomain()
    )
}
