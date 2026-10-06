package com.dc.melodiasmario.feature.currenttrack.presentation.queue

import com.dc.melodiasmario.feature.currenttrack.presentation.state.CurrentTrackUiState

internal object CurrentTrackQueueReducer {
    fun reorder(
        state: CurrentTrackUiState,
        fromIndex: Int,
        toIndex: Int,
    ): CurrentTrackUiState {
        if (fromIndex !in state.queue.tracks.indices || toIndex !in state.queue.tracks.indices) return state
        val tracks = state.queue.tracks.toMutableList()
        val item = tracks.removeAt(fromIndex)
        tracks.add(toIndex, item)

        return state.copy(
            queue = state.queue.copy(
                tracks = tracks,
            )
        )
    }
}
