package com.dc.melodiasmario.feature.currenttrack.presentation

data class CurrentTrackUiState(
    val isLoading: Boolean = false,
    val errorMessage: String? = null,
    val header: CurrentTrackHeaderUiState = CurrentTrackHeaderUiState(),
    val playback: CurrentTrackPlaybackUiState = CurrentTrackPlaybackUiState(),
    val queue: CurrentTrackQueueUiState = CurrentTrackQueueUiState(),
    val addToQueue: CurrentTrackAddToQueueUiState = CurrentTrackAddToQueueUiState(),
    val actions: CurrentTrackActionsUiState = CurrentTrackActionsUiState(),
    val dialog: CurrentTrackDialog = CurrentTrackDialog.None,
)





