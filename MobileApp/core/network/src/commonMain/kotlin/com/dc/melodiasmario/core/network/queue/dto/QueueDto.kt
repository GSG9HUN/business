package com.dc.melodiasmario.core.network.queue.dto

import com.dc.melodiasmario.core.model.queue.Queue
import kotlinx.serialization.Serializable

@Serializable
data class QueueDto(
    val guildId: String = "",
    val trackCount: Int = 0,
    val tracks: List<QueueTrackDto> = emptyList(),
) {
    fun toDomain() = Queue(
        guildId = guildId,
        trackCount = trackCount,
        tracks = tracks.map { it.toDomain() },
    )
}