package com.dc.melodiasmario.core.network.queue.dto

import com.dc.melodiasmario.core.model.currenttrack.Track
import kotlinx.serialization.Serializable

@Serializable
data class QueueTrackDto(
    val position: Int = 0,
    val title: String = "",
    val author: String = "",
    val duration: Int = 0,
    val trackUri: String = "",
    val artworkUri: String? = null,
    val requestedBy: String? = null,
) {
    fun toDomain() = Track(
        id = trackUri.ifBlank { position.toString() },
        title = title,
        artist = author,
        durationSeconds = duration,
        thumbnailUrl = artworkUri,
        requestedBy = requestedBy,
    )
}