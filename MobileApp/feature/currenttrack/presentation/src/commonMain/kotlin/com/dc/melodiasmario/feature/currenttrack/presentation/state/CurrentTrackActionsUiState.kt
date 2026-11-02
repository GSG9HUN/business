package com.dc.melodiasmario.feature.currenttrack.presentation.state

/**
 * Defines current track actions presentation state shared with UI consumers.
 *
 * @property isCommandInFlight Is command in flight.
 * @property isClearQueueLoading Is clear queue loading.
 * @property isShuffleQueueLoading Is shuffle queue loading.
 * @property isPlayPauseLoading Is play pause loading.
 */
data class CurrentTrackActionsUiState(
    val isCommandInFlight: Boolean = false,
    val isClearQueueLoading: Boolean = false,
    val isShuffleQueueLoading: Boolean = false,
    val isPlayPauseLoading: Boolean = false
)
