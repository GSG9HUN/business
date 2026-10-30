package com.dc.melodiasmario.feature.playlisttrack.presentation

import com.dc.melodiasmario.core.presentation.MviEffect

sealed interface PlaylistTrackEffect : MviEffect {
    data object NavigateBack : PlaylistTrackEffect
    data object OperationFailed : PlaylistTrackEffect
    data object SearchFailed : PlaylistTrackEffect
    data object CommandAccepted : PlaylistTrackEffect
    data object TrackAdded : PlaylistTrackEffect
    data object PlaylistLoaded : PlaylistTrackEffect
    data object TrackRemoved : PlaylistTrackEffect
    data object OrderUpdated : PlaylistTrackEffect
    data object CommandTimedOut : PlaylistTrackEffect
}
