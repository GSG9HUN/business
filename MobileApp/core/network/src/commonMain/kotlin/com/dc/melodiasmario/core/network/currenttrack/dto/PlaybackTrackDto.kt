package com.dc.melodiasmario.core.network.currenttrack.dto

import com.dc.melodiasmario.core.model.currenttrack.Track
import kotlinx.serialization.Serializable

/**
 * Represents playback track dto in the API transport contract.
 *
 * @property title Title.
 * @property author Author.
 * @property duration Duration.
 * @property trackUri Track uri.
 * @property artworkUri Optional artwork URI returned by the API.
 * @property requestedBy Requested by.
 * @property isSeekable False for unsupported or older servers.
 * @property playbackOperationId The attempt identity required for safe seek requests.
 */
@Serializable
data class PlaybackTrackDto(
    val title: String = "",
    val author: String = "",
    val duration: Int = 0,
    val trackUri: String = "",
    val artworkUri: String? = null,
    val requestedBy: String? = null,
    val isSeekable: Boolean = false,
    val playbackOperationId: String? = null
) {
    /**
     * Maps this value to domain.
     */
    fun toDomain(): Track = Track(
        id = trackUri,
        title = title,
        artist = author,
        durationSeconds = duration,
        thumbnailUrl = artworkUri,
        requestedBy = requestedBy,
        isSeekable = isSeekable,
        playbackOperationId = playbackOperationId
    )
}
