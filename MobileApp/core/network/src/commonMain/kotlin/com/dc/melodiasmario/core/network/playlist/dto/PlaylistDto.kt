package com.dc.melodiasmario.core.network.playlist.dto

import com.dc.melodiasmario.core.model.playlist.Playlist
import kotlinx.serialization.Serializable

/**
 * Represents playlist dto in the API transport contract.
 *
 * @property id Id.
 * @property name Name.
 * @property songCount Song count.
 * @property duration Duration.
 * @property artworkUri Optional artwork URI returned by the API.
 */
@Serializable
data class PlaylistDto(
    val id: String,
    val name: String,
    val songCount: Int,
    val duration: Int,
    val artworkUri: String? = null
) {
    /**
     * Maps this value to domain.
     */
    fun toDomain(): Playlist {
        return Playlist(
            id = id,
            name = name,
            songCount = songCount,
            duration = duration,
            artworkUri = artworkUri
        )
    }
}
