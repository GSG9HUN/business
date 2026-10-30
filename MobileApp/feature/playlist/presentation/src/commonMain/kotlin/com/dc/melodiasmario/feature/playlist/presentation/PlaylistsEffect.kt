package com.dc.melodiasmario.feature.playlist.presentation

import com.dc.melodiasmario.core.presentation.MviEffect

sealed interface PlaylistsEffect : MviEffect {
    data object ArtworkSaved : PlaylistsEffect
    data object ArtworkSaveFailed : PlaylistsEffect
    data object NavigateToGuildSelector : PlaylistsEffect
    data class NavigateToPlaylistTracks(val playlistId: String) : PlaylistsEffect
    data object PlaylistCreated : PlaylistsEffect
    data object PlaylistCreateFailed : PlaylistsEffect
    data object PlaylistRenamed : PlaylistsEffect
    data object PlaylistRenameFailed : PlaylistsEffect
    data object PlaylistDeleted : PlaylistsEffect
    data object PlaylistDeleteFailed : PlaylistsEffect
}
