package com.dc.melodiasmario.feature.playlist.presentation

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
