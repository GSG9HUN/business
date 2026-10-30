package com.dc.melodiasmario.core.network.playlist.dto

import kotlinx.serialization.Serializable

/**
 * Represents rename playlist request dto in the API transport contract.
 *
 * @property newName New name.
 */
@Serializable
data class RenamePlaylistRequestDto(
    val newName: String
)
