package com.dc.melodiasmario.feature.playlisttrack.presentation

sealed interface PlaylistTrackDialog {
    data object None : PlaylistTrackDialog
    data object AddMusic : PlaylistTrackDialog
    data object More : PlaylistTrackDialog
    data class TrackActions(val orderNumber: Int) : PlaylistTrackDialog
    data class ConfirmRemove(val orderNumber: Int) : PlaylistTrackDialog
}
