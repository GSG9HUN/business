package com.dc.melodiasmario.core.network.queue.dto

import com.dc.melodiasmario.core.model.currenttrack.Track
import kotlinx.serialization.Serializable

/**
 * Represents queue track dto in the API transport contract.
 *
 * @property position Position.
 * @property title Title.
 * @property author Author.
 * @property duration Duration.
 * @property trackUri Track uri.
 * @property artworkUri Optional artwork URI returned by the API.
 * @property requestedBy Requested by.
 */
@Serializable
data class QueueTrackDto(
    val position: Int = 0,
    val title: String = "",
    val author: String = "",
    val duration: Int = 0,
    val trackUri: String = "",
    val artworkUri: String? = null,
    val requestedBy: String? = null
) {
    /**
     * Maps this value to domain.
     */
    fun toDomain(): Track = Track(
        id = trackUri.ifBlank { position.toString() },
        title = title,
        artist = author,
        durationSeconds = duration,
        thumbnailUrl = artworkUri,
        requestedBy = requestedBy
    )
}
