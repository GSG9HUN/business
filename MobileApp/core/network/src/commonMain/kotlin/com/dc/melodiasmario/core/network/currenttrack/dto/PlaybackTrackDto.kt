package com.dc.melodiasmario.core.network.currenttrack.dto

import com.dc.melodiasmario.core.model.currenttrack.Track
import kotlinx.serialization.Serializable

@Serializable
data class PlaybackTrackDto(
    val title: String = "",
    val author: String = "",
    val duration: Int = 0,
    val trackUri: String = "",
    val artworkUri: String? = null,
    val requestedBy: String? = null,
) {
    fun toDomain() = Track(
        id = trackUri,
        title = title,
        artist = author,
        durationSeconds = duration,
        thumbnailUrl = artworkUri,
        requestedBy = requestedBy,
    )
}
