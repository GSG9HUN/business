package com.dc.melodiasmario.core.presentation.queue

interface TrackQueueUiState<Track, State> {
    val tracks: List<Track>

    fun withTracks(tracks: List<Track>): State
}