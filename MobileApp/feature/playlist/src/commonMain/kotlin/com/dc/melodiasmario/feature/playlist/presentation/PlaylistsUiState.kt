package com.dc.melodiasmario.feature.playlist.presentation

import com.dc.melodiasmario.feature.playlist.domain.model.GuildData
import com.dc.melodiasmario.feature.playlist.domain.model.Playlist

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
)

sealed interface PlaylistsDialog {
    data object None : PlaylistsDialog
    data object Create : PlaylistsDialog
    data object Search : PlaylistsDialog
    data class Rename(
        val playlistId: String,
        val playlistName: String,
    ) : PlaylistsDialog
    data class Delete(
        val playlistId: String,
        val playlistName: String,
    ) : PlaylistsDialog
}
