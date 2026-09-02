package com.dc.melodiasmario.feature.playlist.data.remote.dto

import com.dc.melodiasmario.feature.playlist.domain.model.Playlist

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
