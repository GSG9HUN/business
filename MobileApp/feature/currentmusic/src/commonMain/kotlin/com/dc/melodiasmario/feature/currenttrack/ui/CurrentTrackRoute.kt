package com.dc.melodiasmario.feature.currenttrack.ui

import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import com.dc.melodiasmario.core.commonui.feedback.model.MToastData
import com.dc.melodiasmario.core.commonui.feedback.model.MToastType
import com.dc.melodiasmario.core.commonui.feedback.state.MToastHostState
import com.dc.melodiasmario.feature.currentmusic.generated.resources.Res
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_add_to_queue_failed
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_added_to_queue
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_clear_queue_failed
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_cleared_queue
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_load_failed
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_move_down_failed
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_move_up_failed
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_moved_down_in_queue
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_moved_up_in_queue
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_next_track_failed
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_play_pause_toggle_failed
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_playback_updated
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_previous_track_failed
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_queue_shuffled
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_remove_from_queue_failed
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_removed_from_queue
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_repeat_mode_change_failed
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_repeat_mode_changed
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_shuffle_queue_failed
import com.dc.melodiasmario.feature.currenttrack.presentation.CurrentTrackEffect
import com.dc.melodiasmario.feature.currenttrack.presentation.CurrentTrackEvent
import com.dc.melodiasmario.feature.currenttrack.presentation.CurrentTrackViewModel
import kotlinx.coroutines.delay
import org.jetbrains.compose.resources.stringResource
import org.koin.compose.viewmodel.koinViewModel

@Composable
fun CurrentTrackRoute(
    modifier: Modifier = Modifier,
    viewModel: CurrentTrackViewModel = koinViewModel(),
    guildId: String,
    onGuildClicked: () -> Unit,
    onProfileClicked: () -> Unit,
    toastHostState: MToastHostState,
) {
    val uiState by viewModel.uiState.collectAsState()
    val addedToQueueText = stringResource(Res.string.currenttrack_added_to_queue)
    val removedFromQueueText = stringResource(Res.string.currenttrack_removed_from_queue)
    val clearedQueueText = stringResource(Res.string.currenttrack_cleared_queue)
    val repeatModeChangedText = stringResource(Res.string.currenttrack_repeat_mode_changed)
    val queueShuffledText = stringResource(Res.string.currenttrack_queue_shuffled)
    val playbackUpdatedText = stringResource(Res.string.currenttrack_playback_updated)
    val movedUpInQueueText = stringResource(Res.string.currenttrack_moved_up_in_queue)
    val movedDownInQueueText = stringResource(Res.string.currenttrack_moved_down_in_queue)
    val loadFailedText = stringResource(Res.string.currenttrack_load_failed)
    val addToQueueFailedText = stringResource(Res.string.currenttrack_add_to_queue_failed)
    val removeFromQueueFailedText = stringResource(Res.string.currenttrack_remove_from_queue_failed)
    val clearQueueFailedText = stringResource(Res.string.currenttrack_clear_queue_failed)
    val nextTrackFailedText = stringResource(Res.string.currenttrack_next_track_failed)
    val previousTrackFailedText = stringResource(Res.string.currenttrack_previous_track_failed)
    val repeatModeChangeFailedText =
        stringResource(Res.string.currenttrack_repeat_mode_change_failed)
    val shuffleQueueFailedText = stringResource(Res.string.currenttrack_shuffle_queue_failed)
    val playPauseToggleFailedText = stringResource(Res.string.currenttrack_play_pause_toggle_failed)
    val moveUpFailedText = stringResource(Res.string.currenttrack_move_up_failed)
    val moveDownFailedText = stringResource(Res.string.currenttrack_move_down_failed)

    LaunchedEffect(guildId) {
        viewModel.onEvent(CurrentTrackEvent.LoadCurrentTrack(guildId))
    }

    LaunchedEffect(guildId) {
        while (true) {
            delay(CurrentTrackSyncIntervalMillis)
            viewModel.onEvent(CurrentTrackEvent.SyncCurrentTrack)
        }
    }

    LaunchedEffect(viewModel) {
        viewModel.effect.collect { effect ->
            when (effect) {
                CurrentTrackEffect.NavigateToGuildSelector -> onGuildClicked()
                CurrentTrackEffect.NavigateToProfile -> onProfileClicked()

                CurrentTrackEffect.AddedToQueue -> toastHostState.showToast(
                    MToastData(
                        message = addedToQueueText,
                        type = MToastType.Success
                    )
                )

                CurrentTrackEffect.RemovedFromQueue -> toastHostState.showToast(
                    MToastData(
                        message = removedFromQueueText,
                        type = MToastType.Success
                    )
                )

                CurrentTrackEffect.ClearedQueue -> toastHostState.showToast(
                    MToastData(
                        message = clearedQueueText,
                        type = MToastType.Success
                    )
                )

                CurrentTrackEffect.RepeatModeChanged -> toastHostState.showToast(
                    MToastData(
                        message = repeatModeChangedText,
                        type = MToastType.Success
                    )
                )

                CurrentTrackEffect.QueueShuffled -> toastHostState.showToast(
                    MToastData(
                        message = queueShuffledText,
                        type = MToastType.Success
                    )
                )

                CurrentTrackEffect.PlayPauseToggled -> toastHostState.showToast(
                    MToastData(
                        message = playbackUpdatedText,
                        type = MToastType.Success
                    )
                )

                CurrentTrackEffect.MovedUpInQueue -> toastHostState.showToast(
                    MToastData(
                        message = movedUpInQueueText,
                        type = MToastType.Success
                    )
                )

                CurrentTrackEffect.MovedDownInQueue -> toastHostState.showToast(
                    MToastData(
                        message = movedDownInQueueText,
                        type = MToastType.Success
                    )
                )

                CurrentTrackEffect.LoadCurrentTrackFailed -> toastHostState.showToast(
                    MToastData(
                        message = loadFailedText,
                        type = MToastType.Error
                    )
                )

                CurrentTrackEffect.AddToQueueFailed -> toastHostState.showToast(
                    MToastData(
                        message = addToQueueFailedText,
                        type = MToastType.Error
                    )
                )

                CurrentTrackEffect.RemoveFromQueueFailed -> toastHostState.showToast(
                    MToastData(
                        message = removeFromQueueFailedText,
                        type = MToastType.Error
                    )
                )

                CurrentTrackEffect.ClearQueueFailed -> toastHostState.showToast(
                    MToastData(
                        message = clearQueueFailedText,
                        type = MToastType.Error
                    )
                )

                CurrentTrackEffect.NextTrackFailed -> toastHostState.showToast(
                    MToastData(
                        message = nextTrackFailedText,
                        type = MToastType.Error
                    )
                )

                CurrentTrackEffect.PreviousTrackFailed -> toastHostState.showToast(
                    MToastData(
                        message = previousTrackFailedText,
                        type = MToastType.Error
                    )
                )

                CurrentTrackEffect.RepeatModeChangeFailed -> toastHostState.showToast(
                    MToastData(
                        message = repeatModeChangeFailedText,
                        type = MToastType.Error
                    )
                )

                CurrentTrackEffect.ShuffleQueueFailed -> toastHostState.showToast(
                    MToastData(
                        message = shuffleQueueFailedText,
                        type = MToastType.Error
                    )
                )

                CurrentTrackEffect.PlayPauseToggleFailed -> toastHostState.showToast(
                    MToastData(
                        message = playPauseToggleFailedText,
                        type = MToastType.Error
                    )
                )

                CurrentTrackEffect.MoveUpInQueueFailed -> toastHostState.showToast(
                    MToastData(
                        message = moveUpFailedText,
                        type = MToastType.Error
                    )
                )

                CurrentTrackEffect.MoveDownInQueueFailed -> toastHostState.showToast(
                    MToastData(
                        message = moveDownFailedText,
                        type = MToastType.Error
                    )
                )
            }
        }
    }

    CurrentTrackScreen(
        modifier = modifier,
        uiState = uiState,
        onEvent = viewModel::onEvent
    )
}

private const val CurrentTrackSyncIntervalMillis = 15_000L
