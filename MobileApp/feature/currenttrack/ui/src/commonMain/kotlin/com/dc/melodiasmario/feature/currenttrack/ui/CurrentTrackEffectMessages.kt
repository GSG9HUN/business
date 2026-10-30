package com.dc.melodiasmario.feature.currenttrack.ui

import androidx.compose.runtime.Composable
import com.dc.melodiasmario.core.commonui.feedback.model.MToastData
import com.dc.melodiasmario.core.commonui.feedback.model.MToastType
import com.dc.melodiasmario.feature.currenttrack.generated.resources.Res
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_add_to_queue_failed
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_added_to_queue
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_clear_queue_failed
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_cleared_queue
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_load_failed
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_move_to_index_failed
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_next_track_failed
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_play_pause_toggle_failed
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_playback_updated
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_previous_track_failed
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_queue_shuffled
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_remove_from_queue_failed
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_removed_from_queue
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_repeat_mode_change_failed
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_repeat_mode_changed
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_search_failed
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_shuffle_queue_failed
import com.dc.melodiasmario.feature.currenttrack.presentation.CurrentTrackEffect
import org.jetbrains.compose.resources.stringResource

/**
 * Provides current track effect messages for the ui layer.
 */
internal data class CurrentTrackEffectMessages(
    private val commandMessages: CurrentTrackCommandMessages,
    private val addedToQueue: String,
    private val removedFromQueue: String,
    private val clearedQueue: String,
    private val repeatModeChanged: String,
    private val queueShuffled: String,
    private val playbackUpdated: String,
    private val loadFailed: String,
    private val searchFailed: String,
    private val addToQueueFailed: String,
    private val removeFromQueueFailed: String,
    private val clearQueueFailed: String,
    private val nextTrackFailed: String,
    private val previousTrackFailed: String,
    private val repeatModeChangeFailed: String,
    private val shuffleQueueFailed: String,
    private val playPauseToggleFailed: String,
    private val moveToIndexFailed: String
) {
    /**
     * Maps the supplied effect to localized toast data, or returns null when no toast is needed.
     */
    fun toastDataFor(effect: CurrentTrackEffect): MToastData? = when (effect) {
        CurrentTrackEffect.NavigateToGuildSelector,
        CurrentTrackEffect.NavigateToProfile -> null

        CurrentTrackEffect.BotCommandAccepted -> info(message = commandMessages.accepted)

        is CurrentTrackEffect.BotCommandSucceeded -> success(
            message = commandMessages.successMessageFor(commandType = effect.commandType)
        )

        is CurrentTrackEffect.BotCommandFailed -> error(
            message = commandMessages.errorMessageFor(commandType = effect.commandType, errorKey = effect.errorKey)
        )

        CurrentTrackEffect.AddedToQueue -> success(message = addedToQueue)
        CurrentTrackEffect.RemovedFromQueue -> success(message = removedFromQueue)
        CurrentTrackEffect.ClearedQueue -> success(message = clearedQueue)
        CurrentTrackEffect.RepeatModeChanged -> success(message = repeatModeChanged)
        CurrentTrackEffect.QueueShuffled -> success(message = queueShuffled)
        CurrentTrackEffect.PlayPauseToggled -> success(message = playbackUpdated)
        CurrentTrackEffect.MoveToIndexFailed -> error(message = moveToIndexFailed)
        CurrentTrackEffect.LoadCurrentTrackFailed -> error(message = loadFailed)
        CurrentTrackEffect.SearchFailed -> error(message = searchFailed)
        CurrentTrackEffect.AddToQueueFailed -> error(message = addToQueueFailed)
        CurrentTrackEffect.RemoveFromQueueFailed -> error(message = removeFromQueueFailed)
        CurrentTrackEffect.ClearQueueFailed -> error(message = clearQueueFailed)
        CurrentTrackEffect.NextTrackFailed -> error(message = nextTrackFailed)
        CurrentTrackEffect.PreviousTrackFailed -> error(message = previousTrackFailed)
        CurrentTrackEffect.RepeatModeChangeFailed -> error(message = repeatModeChangeFailed)
        CurrentTrackEffect.ShuffleQueueFailed -> error(message = shuffleQueueFailed)
        CurrentTrackEffect.PlayPauseToggleFailed -> error(message = playPauseToggleFailed)
    }

    private fun success(message: String) = MToastData(message = message, type = MToastType.Success)
    private fun error(message: String) = MToastData(message = message, type = MToastType.Error)
    private fun info(message: String) = MToastData(message = message, type = MToastType.Info)
}

/**
 * Provides current track effect messages for the current composition.
 */
@Composable
internal fun currentTrackEffectMessages(): CurrentTrackEffectMessages = CurrentTrackEffectMessages(
    commandMessages = currentTrackCommandMessages(),
    addedToQueue = stringResource(resource = Res.string.currenttrack_added_to_queue),
    removedFromQueue = stringResource(resource = Res.string.currenttrack_removed_from_queue),
    clearedQueue = stringResource(resource = Res.string.currenttrack_cleared_queue),
    repeatModeChanged = stringResource(resource = Res.string.currenttrack_repeat_mode_changed),
    queueShuffled = stringResource(resource = Res.string.currenttrack_queue_shuffled),
    playbackUpdated = stringResource(resource = Res.string.currenttrack_playback_updated),
    loadFailed = stringResource(resource = Res.string.currenttrack_load_failed),
    searchFailed = stringResource(resource = Res.string.currenttrack_search_failed),
    addToQueueFailed = stringResource(resource = Res.string.currenttrack_add_to_queue_failed),
    removeFromQueueFailed = stringResource(resource = Res.string.currenttrack_remove_from_queue_failed),
    clearQueueFailed = stringResource(resource = Res.string.currenttrack_clear_queue_failed),
    nextTrackFailed = stringResource(resource = Res.string.currenttrack_next_track_failed),
    previousTrackFailed = stringResource(resource = Res.string.currenttrack_previous_track_failed),
    repeatModeChangeFailed = stringResource(resource = Res.string.currenttrack_repeat_mode_change_failed),
    shuffleQueueFailed = stringResource(resource = Res.string.currenttrack_shuffle_queue_failed),
    playPauseToggleFailed = stringResource(resource = Res.string.currenttrack_play_pause_toggle_failed),
    moveToIndexFailed = stringResource(resource = Res.string.currenttrack_move_to_index_failed)
)
