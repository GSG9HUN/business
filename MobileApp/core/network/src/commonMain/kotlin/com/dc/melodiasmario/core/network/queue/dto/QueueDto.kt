package com.dc.melodiasmario.core.network.queue.dto

import com.dc.melodiasmario.core.model.queue.Queue
import kotlinx.serialization.Serializable

/**
 * Represents queue dto in the API transport contract.
 *
 * @property guildId Identifier of the guild owning this value.
 * @property trackCount Track count.
 * @property tracks Tracks.
 */
@Serializable
data class QueueDto(
    val guildId: String = "",
    val trackCount: Int = 0,
    val tracks: List<QueueTrackDto> = emptyList()
) {
    /**
     * Maps this value to domain.
     */
    fun toDomain(): Queue = Queue(
        guildId = guildId,
        trackCount = trackCount,
        tracks = tracks.map { track -> track.toDomain() }
    )
}
