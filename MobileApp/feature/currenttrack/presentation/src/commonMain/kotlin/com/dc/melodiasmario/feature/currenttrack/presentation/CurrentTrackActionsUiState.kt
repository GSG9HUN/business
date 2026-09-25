package com.dc.melodiasmario.feature.currenttrack.presentation
data class CurrentTrackActionsUiState(
    val isClearQueueLoading: Boolean = false,
    val isShuffleQueueLoading: Boolean = false,
    val isPlayPauseLoading: Boolean = false,
)
