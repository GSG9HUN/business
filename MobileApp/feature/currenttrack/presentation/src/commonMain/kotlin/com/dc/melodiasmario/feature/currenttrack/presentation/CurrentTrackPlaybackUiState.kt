package com.dc.melodiasmario.feature.currenttrack.presentation

import com.dc.melodiasmario.core.model.currenttrack.CurrentTrack
import com.dc.melodiasmario.core.model.currenttrack.RepeatMode

data class CurrentTrackPlaybackUiState(
    val currentTrack: CurrentTrack? = null,
    val isPlaying: Boolean = false,
    val repeatMode: RepeatMode = RepeatMode.NONE,
)