package com.dc.melodiasmario.core.presentation.queue

class TrackQueueReducer {

    fun <Track, State> reorder(state: State, fromIndex: Int, toIndex: Int): State where State : TrackQueueUiState<Track, State> {
        if (fromIndex == toIndex
            || fromIndex !in state.tracks.indices
            || toIndex !in state.tracks.indices
        ) {
            return state
        }

        val tracks = state.tracks.toMutableList()
        val movedTrack = tracks.removeAt(index = fromIndex)
        tracks.add(index = toIndex, element = movedTrack)
        return state.withTracks(tracks = tracks)
    }
}