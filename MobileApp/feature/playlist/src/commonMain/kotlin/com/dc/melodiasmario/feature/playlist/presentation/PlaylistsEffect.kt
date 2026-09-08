package com.dc.melodiasmario.feature.playlist.presentation

sealed interface PlaylistsEffect {
    data object NavigateToGuildSelector : PlaylistsEffect
    data class NavigateToPlaylistSongs(val playlistId: String) : PlaylistsEffect
}
