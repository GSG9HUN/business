package com.dc.melodiasmario.core.network.playlisttrack.dto

import com.dc.melodiasmario.core.model.playlist.PlaylistDetail
import kotlinx.serialization.Serializable

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
    fun toDomain() = PlaylistDetail(
        id = id,
        guildId = guildId,
        name = name,
        artworkUri = artworkUri,
        tracks = tracks.sortedBy { it.orderNumber }.map { it.toDomain() }
    )
}
