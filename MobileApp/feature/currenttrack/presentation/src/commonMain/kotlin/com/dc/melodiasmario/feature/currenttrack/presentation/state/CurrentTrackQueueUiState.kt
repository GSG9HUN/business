package com.dc.melodiasmario.feature.currenttrack.presentation.state

import com.dc.melodiasmario.core.model.currenttrack.Track

/**
 * Defines current track queue presentation state shared with UI consumers.
 *
 * @property tracks Tracks.
 */
data class CurrentTrackQueueUiState(
    val tracks: List<Track> = emptyList()
)
