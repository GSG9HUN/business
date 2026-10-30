package com.dc.melodiasmario.core.model.playlist

data class PlaylistDetail(
    val id: String,
    val guildId: String,
    val name: String,
    val tracks: List<PlaylistTrack>,
    val artworkUri: String? = null
)
