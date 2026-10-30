package com.dc.melodiasmario.core.network.playlist.dto

import kotlinx.serialization.Serializable

/**
 * Represents create playlist request dto in the API transport contract.
 *
 * @property playlistName Playlist name.
 */
@Serializable
data class CreatePlaylistRequestDto(
    val playlistName: String
)
