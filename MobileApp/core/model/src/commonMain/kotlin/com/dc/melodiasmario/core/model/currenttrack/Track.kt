package com.dc.melodiasmario.core.model.currenttrack

data class Track(
    val id: String = "",
    val title: String = "",
    val artist: String = "",
    val durationSeconds: Int = 0,
    val thumbnailUrl: String? = null,
    val requestedBy: String? = null,
)
