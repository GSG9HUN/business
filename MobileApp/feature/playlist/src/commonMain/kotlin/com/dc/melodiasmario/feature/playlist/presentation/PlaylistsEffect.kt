package com.dc.melodiasmario.feature.playlist.presentation

sealed interface PlaylistsEffect {
    data object NavigateToGuildSelector : PlaylistsEffect
    data class NavigateToPlaylistSongs(val playlistId: String) : PlaylistsEffect
    data object PlaylistCreated : PlaylistsEffect
    data object PlaylistCreateFailed : PlaylistsEffect
    data object PlaylistRenamed : PlaylistsEffect
    data object PlaylistRenameFailed : PlaylistsEffect
    data object PlaylistDeleted : PlaylistsEffect
    data object PlaylistDeleteFailed : PlaylistsEffect
}
