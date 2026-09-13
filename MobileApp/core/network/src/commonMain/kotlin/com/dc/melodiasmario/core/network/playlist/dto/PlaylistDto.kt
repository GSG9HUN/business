package com.dc.melodiasmario.core.network.playlist.dto

import com.dc.melodiasmario.core.model.playlist.Playlist
import kotlinx.serialization.Serializable

@Serializable
data class PlaylistDto(
    val id: String,
    val name: String,
    val songCount: Int,
    val duration: Int,
) {
    fun toDomain(): Playlist {
        return Playlist(
            id = id,
            name = name,
            songCount = songCount,
            duration = duration,
        )
    }
}

@Serializable
data class RenamePlaylistRequestDto(
    val newName: String
)

@Serializable
data class CreatePlaylistRequestDto(
    val playlistName: String
)
