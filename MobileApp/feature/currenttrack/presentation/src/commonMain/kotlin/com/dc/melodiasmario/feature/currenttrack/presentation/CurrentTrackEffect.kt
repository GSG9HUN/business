package com.dc.melodiasmario.feature.currenttrack.presentation

import com.dc.melodiasmario.core.common.presentation.MviEffect

sealed interface CurrentTrackEffect : MviEffect {
    data object NavigateToGuildSelector : CurrentTrackEffect
    data object NavigateToProfile : CurrentTrackEffect
    data object LoadCurrentTrackFailed : CurrentTrackEffect
    data object AddedToQueue : CurrentTrackEffect
    data object AddToQueueFailed : CurrentTrackEffect
    data object RemovedFromQueue : CurrentTrackEffect
    data object RemoveFromQueueFailed : CurrentTrackEffect
    data object ClearedQueue : CurrentTrackEffect
    data object ClearQueueFailed : CurrentTrackEffect
    data object NextTrackFailed : CurrentTrackEffect
    data object PreviousTrackFailed : CurrentTrackEffect
    data object RepeatModeChanged : CurrentTrackEffect
    data object RepeatModeChangeFailed : CurrentTrackEffect
    data object QueueShuffled : CurrentTrackEffect
    data object ShuffleQueueFailed : CurrentTrackEffect
    data object PlayPauseToggled : CurrentTrackEffect
    data object PlayPauseToggleFailed : CurrentTrackEffect
    data object MovedUpInQueue : CurrentTrackEffect
    data object MoveUpInQueueFailed : CurrentTrackEffect
    data object MovedDownInQueue : CurrentTrackEffect
    data object MoveDownInQueueFailed : CurrentTrackEffect
}
