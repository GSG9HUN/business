package com.dc.melodiasmario.core.model.playlist

/**
 * Represents playlist track shared between application layers.
 *
 * @property orderNumber Stored order number of the playlist track.
 * @property title Title.
 * @property author Author.
 * @property durationSeconds Track duration in seconds.
 * @property trackUri Track uri.
 * @property artworkUri Optional artwork URI returned by the API.
 */
data class PlaylistTrack(
    val orderNumber: Int,
    val title: String,
    val author: String,
    val durationSeconds: Int,
    val trackUri: String,
    val artworkUri: String? = null
)
