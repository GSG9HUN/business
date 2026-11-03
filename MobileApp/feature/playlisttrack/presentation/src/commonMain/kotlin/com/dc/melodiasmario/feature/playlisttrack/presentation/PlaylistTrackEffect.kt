package com.dc.melodiasmario.feature.playlisttrack.presentation

import com.dc.melodiasmario.core.presentation.MviEffect

/**
 * Defines one-time playlist track presentation outcomes.
 */
sealed interface PlaylistTrackEffect : MviEffect {
    /**
     * Describes the navigate back variant of [PlaylistTrackEffect].
     */
    data object NavigateBack : PlaylistTrackEffect

    /**
     * Describes the operation failed variant of [PlaylistTrackEffect].
     */
    data object OperationFailed : PlaylistTrackEffect

    /**
     * Describes the search failed variant of [PlaylistTrackEffect].
     */
    data object SearchFailed : PlaylistTrackEffect

    /**
     * Describes the command accepted variant of [PlaylistTrackEffect].
     */
    data object CommandAccepted : PlaylistTrackEffect

    /**
     * Describes the track added variant of [PlaylistTrackEffect].
     */
    data object TrackAdded : PlaylistTrackEffect

    /**
     * Describes the playlist loaded variant of [PlaylistTrackEffect].
     */
    data object PlaylistLoaded : PlaylistTrackEffect

    /**
     * Describes the track removed variant of [PlaylistTrackEffect].
     */
    data object TrackRemoved : PlaylistTrackEffect

    /**
     * Describes the order updated variant of [PlaylistTrackEffect].
     */
    data object OrderUpdated : PlaylistTrackEffect

    /**
     * Describes the command timed out variant of [PlaylistTrackEffect].
     */
    data object CommandTimedOut : PlaylistTrackEffect
}
