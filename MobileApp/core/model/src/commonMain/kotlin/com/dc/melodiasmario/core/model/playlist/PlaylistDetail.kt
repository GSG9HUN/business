package com.dc.melodiasmario.core.model.playlist

/**
 * Represents playlist detail shared between application layers.
 *
 * @property id Id.
 * @property guildId Identifier of the guild owning this value.
 * @property name Name.
 * @property tracks Tracks.
 * @property artworkUri Optional artwork URI returned by the API.
 */
data class PlaylistDetail(
    val id: String,
    val guildId: String,
    val name: String,
    val tracks: List<PlaylistTrack>,
    val artworkUri: String? = null
)
