package com.dc.melodiasmario.core.network.playlist.dto

import kotlinx.serialization.Serializable

/**
 * Represents add playlist track request dto in the API transport contract.
 *
 * @property songUrl Song url.
 */
@Serializable
data class AddPlaylistTrackRequestDto(val songUrl: String)
