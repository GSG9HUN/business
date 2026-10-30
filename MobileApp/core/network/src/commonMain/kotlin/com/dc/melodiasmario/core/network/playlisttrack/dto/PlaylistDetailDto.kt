package com.dc.melodiasmario.core.network.playlisttrack.dto

import com.dc.melodiasmario.core.model.playlist.PlaylistDetail
import kotlinx.serialization.Serializable

/**
 * Represents playlist detail dto in the API transport contract.
 *
 * @property id Id.
 * @property guildId Identifier of the guild owning this value.
 * @property name Name.
 * @property songCount Song count.
 * @property duration Duration.
 * @property tracks Tracks.
 * @property artworkUri Optional artwork URI returned by the API.
 */
@Serializable
data class PlaylistDetailDto(
    val id: String,
    val guildId: String,
    val name: String,
    val songCount: Int,
    val duration: Int,
    val tracks: List<PlaylistTrackDto>,
    val artworkUri: String? = null
) {
    /**
     * Maps this value to domain.
     */
    fun toDomain(): PlaylistDetail = PlaylistDetail(
        id = id,
        guildId = guildId,
        name = name,
        artworkUri = artworkUri,
        tracks = tracks.sortedBy { playlistTrack -> playlistTrack.orderNumber }
            .map { track -> track.toDomain() }
    )
}
