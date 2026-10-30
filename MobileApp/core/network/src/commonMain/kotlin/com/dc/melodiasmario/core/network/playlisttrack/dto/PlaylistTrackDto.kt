package com.dc.melodiasmario.core.network.playlisttrack.dto

import com.dc.melodiasmario.core.model.playlist.PlaylistTrack
import kotlinx.serialization.Serializable

@Serializable
data class PlaylistTrackDto(
    val orderNumber: Int,
    val title: String,
    val author: String,
    val duration: Int,
    val trackUri: String,
    val artworkUri: String? = null
) {
    fun toDomain() = PlaylistTrack(
        orderNumber = orderNumber,
        title = title,
        author = author,
        durationSeconds = duration,
        trackUri = trackUri,
        artworkUri = artworkUri
    )
}
