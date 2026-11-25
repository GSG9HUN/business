package com.dc.melodiasmario.feature.currenttrack.presentation.state

/**
 * Defines current track actions presentation state shared with UI consumers.
 *
 * @property isCommandInFlight Is command in flight.
 * @property isClearQueueLoading Is clear queue loading.
 * @property isShuffleQueueLoading Is shuffle queue loading.
 * @property isPlayPauseLoading Is play pause loading.
 * @property isVolumeLoading Whether a volume request is awaiting acceptance or terminal completion.
 * @property isSeekLoading Whether a seek request is awaiting acceptance or terminal completion.
 */
data class CurrentTrackActionsUiState(
    val isCommandInFlight: Boolean = false,
    val isClearQueueLoading: Boolean = false,
    val isShuffleQueueLoading: Boolean = false,
    val isPlayPauseLoading: Boolean = false,
    val isVolumeLoading: Boolean = false,
    val isSeekLoading: Boolean = false
)
