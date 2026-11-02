package com.dc.melodiasmario.core.presentation.queue

/**
 * Defines track queue presentation state shared with UI consumers.
 */
interface TrackQueueUiState<Track, State> {
    /**
     * Tracks exposed by track queue ui state.
     */
    val tracks: List<Track>

    /**
     * Returns state containing the supplied ordered track list.
     */
    fun withTracks(tracks: List<Track>): State
}
