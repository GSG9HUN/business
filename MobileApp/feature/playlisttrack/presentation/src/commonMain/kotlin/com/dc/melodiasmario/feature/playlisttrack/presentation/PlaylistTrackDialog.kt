package com.dc.melodiasmario.feature.playlisttrack.presentation

/**
 * Provides playlist track dialog for the presentation layer.
 */
sealed interface PlaylistTrackDialog {
    /**
     * Describes the none variant of [PlaylistTrackDialog].
     */
    data object None : PlaylistTrackDialog

    /**
     * Describes the add music variant of [PlaylistTrackDialog].
     */
    data object AddMusic : PlaylistTrackDialog

    /**
     * Describes the more variant of [PlaylistTrackDialog].
     */
    data object More : PlaylistTrackDialog

    /**
     * Describes the track actions variant of [PlaylistTrackDialog].
     *
     * @property orderNumber Stored order number of the playlist track.
     */
    data class TrackActions(val orderNumber: Int) : PlaylistTrackDialog

    /**
     * Describes the confirm remove variant of [PlaylistTrackDialog].
     *
     * @property orderNumber Stored order number of the playlist track.
     */
    data class ConfirmRemove(val orderNumber: Int) : PlaylistTrackDialog
}
