package com.dc.melodiasmario.core.network.playlist.dto

import kotlinx.serialization.Serializable

/**
 * Preserves the displayed queue order, including repeated track URLs.
 *
 * @property playlistName Playlist name.
 * @property trackUrls Track urls.
 */
@Serializable
data class CreatePlaylistFromTracksRequestDto(
    val playlistName: String,
    val trackUrls: List<String>
)
