package com.dc.melodiasmario.core.model.playlist

/**
 * Represents playlist shared between application layers.
 *
 * @property id Id.
 * @property name Name.
 * @property songCount Song count.
 * @property duration Duration.
 * @property artworkUri Optional artwork URI returned by the API.
 */
data class Playlist(
    val id: String = "",
    val name: String = "",
    val songCount: Int = 0,
    val duration: Int = 0,
    val artworkUri: String? = null
)
