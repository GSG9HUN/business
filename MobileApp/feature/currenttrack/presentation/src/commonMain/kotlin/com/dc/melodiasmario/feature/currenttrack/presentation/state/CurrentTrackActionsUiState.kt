package com.dc.melodiasmario.feature.currenttrack.presentation.state

data class CurrentTrackActionsUiState(
    val isCommandInFlight: Boolean = false,
    val isClearQueueLoading: Boolean = false,
    val isShuffleQueueLoading: Boolean = false,
    val isPlayPauseLoading: Boolean = false,
)
