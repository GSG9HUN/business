package com.dc.melodiasmario.feature.currenttrack.presentation.state

import com.dc.melodiasmario.core.model.currenttrack.Track

data class CurrentTrackQueueUiState(
    val tracks: List<Track> = emptyList(),
)
