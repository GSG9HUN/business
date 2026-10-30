package com.dc.melodiasmario.feature.playlist.presentation

import com.dc.melodiasmario.core.model.playlist.GuildData
import com.dc.melodiasmario.core.model.playlist.Playlist

/**
 * Defines playlists presentation state shared with UI consumers.
 *
 * @property isLoading Is loading.
 * @property errorMessage Error message.
 * @property playlists Playlists.
 * @property filteredPlaylists Filtered playlists.
 * @property guildData Guild data.
 * @property searchQuery Search query.
 * @property dialog Dialog.
 * @property playlistNameDraft Playlist name draft.
 * @property searchDraft Search draft.
 * @property artworkDraft Artwork draft.
 */
data class PlaylistsUiState(
    val isLoading: Boolean = false,
    val errorMessage: String? = null,
    val playlists: List<Playlist> = emptyList(),
    val filteredPlaylists: List<Playlist> = emptyList(),
    val guildData: GuildData = GuildData(),
    val searchQuery: String = "",
    val dialog: PlaylistsDialog = PlaylistsDialog.None,
    val playlistNameDraft: String = "",
    val searchDraft: String = "",
    val artworkDraft: String = ""
)
