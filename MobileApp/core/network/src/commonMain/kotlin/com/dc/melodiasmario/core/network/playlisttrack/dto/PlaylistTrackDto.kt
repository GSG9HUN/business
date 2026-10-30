package com.dc.melodiasmario.core.network.playlisttrack.dto

import com.dc.melodiasmario.core.model.playlist.PlaylistTrack
import kotlinx.serialization.Serializable

/**
 * Represents playlist track dto in the API transport contract.
 *
 * @property orderNumber Stored order number of the playlist track.
 * @property title Title.
 * @property author Author.
 * @property duration Duration.
 * @property trackUri Track uri.
 * @property artworkUri Optional artwork URI returned by the API.
 */
@Serializable
data class PlaylistTrackDto(
    val orderNumber: Int,
    val title: String,
    val author: String,
    val duration: Int,
    val trackUri: String,
    val artworkUri: String? = null
) {
    /**
     * Maps this value to domain.
     */
    fun toDomain(): PlaylistTrack = PlaylistTrack(
        orderNumber = orderNumber,
        title = title,
        author = author,
        durationSeconds = duration,
        trackUri = trackUri,
        artworkUri = artworkUri
    )
}
