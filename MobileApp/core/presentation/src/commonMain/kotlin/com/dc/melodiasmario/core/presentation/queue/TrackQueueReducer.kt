package com.dc.melodiasmario.core.presentation.queue

/**
 * Creates updated track queue presentation state from the supplied changes.
 */
class TrackQueueReducer {

    /**
     * Returns queue state after moving a track between the supplied indexes.
     */
    fun <Track, State> reorder(uiState: State, fromIndex: Int, toIndex: Int): State where State : TrackQueueUiState<Track, State> {
        if (fromIndex == toIndex
            || fromIndex !in uiState.tracks.indices
            || toIndex !in uiState.tracks.indices
        ) {
            return uiState
        }

        val tracks = uiState.tracks.toMutableList()
        val movedTrack = tracks.removeAt(index = fromIndex)
        tracks.add(index = toIndex, element = movedTrack)
        return uiState.withTracks(tracks = tracks)
    }
}
