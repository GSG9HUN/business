package com.dc.melodiasmario.core.model.playlist

data class PlaylistTrack(
    val orderNumber: Int,
    val title: String,
    val author: String,
    val durationSeconds: Int,
    val trackUri: String,
    val artworkUri: String? = null
)
