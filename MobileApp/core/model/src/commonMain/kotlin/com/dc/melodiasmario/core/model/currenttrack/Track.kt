package com.dc.melodiasmario.core.model.currenttrack

/**
 * Represents track shared between application layers.
 *
 * @property id Id.
 * @property title Title.
 * @property artist Artist.
 * @property durationSeconds Track duration in seconds.
 * @property thumbnailUrl Optional track artwork URL.
 * @property requestedBy Requested by.
 */
data class Track(
    val id: String = "",
    val title: String = "",
    val artist: String = "",
    val durationSeconds: Int = 0,
    val thumbnailUrl: String? = null,
    val requestedBy: String? = null
)
