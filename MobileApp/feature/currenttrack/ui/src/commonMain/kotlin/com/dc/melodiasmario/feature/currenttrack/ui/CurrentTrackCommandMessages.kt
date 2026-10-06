package com.dc.melodiasmario.feature.currenttrack.ui

import androidx.compose.runtime.Composable
import com.dc.melodiasmario.core.commonui.feedback.model.MToastData
import com.dc.melodiasmario.core.commonui.feedback.model.MToastType
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommandTypes
import com.dc.melodiasmario.feature.currenttrack.generated.resources.Res
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_add_to_queue_failed
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_added_to_queue
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_bot_command_accepted
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_bot_command_error_discord_context_not_found
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_bot_command_error_interrupted
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_bot_command_error_invalid_track_index
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_bot_command_error_invalid_track_number
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_bot_command_error_lavalink
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_bot_command_error_no_current_or_queued_track
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_bot_command_error_no_current_track
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_bot_command_error_no_previous_track
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_bot_command_error_play_not_implemented
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_bot_command_error_player_not_found
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_bot_command_error_queue_too_small
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_bot_command_error_repeat_already_enabled
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_bot_command_error_repeat_list_already_enabled
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_bot_command_error_unknown
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_bot_command_error_user_not_in_voice_channel
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_clear_queue_failed
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_cleared_queue
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_move_to_index_failed
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_moved_in_queue
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_load_failed
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_next_track_failed
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_play_pause_toggle_failed
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_playback_updated
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_previous_track_failed
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_queue_shuffled
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_remove_from_queue_failed
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_removed_from_queue
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_repeat_mode_change_failed
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_repeat_mode_changed
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_shuffle_queue_failed
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_search_failed
import com.dc.melodiasmario.feature.currenttrack.presentation.CurrentTrackEffect
import org.jetbrains.compose.resources.stringResource

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
    private val moveToIndexFailed: String,
) {
    fun toastDataFor(effect: CurrentTrackEffect): MToastData? = when (effect) {
        CurrentTrackEffect.NavigateToGuildSelector,
        CurrentTrackEffect.NavigateToProfile -> null

        CurrentTrackEffect.BotCommandAccepted -> info(commandMessages.accepted)

        is CurrentTrackEffect.BotCommandSucceeded -> success(
            commandMessages.successMessageFor(effect.commandType)
        )

        is CurrentTrackEffect.BotCommandFailed -> error(
            commandMessages.errorMessageFor(effect.commandType, effect.errorKey)
        )

        CurrentTrackEffect.AddedToQueue -> success(addedToQueue)
        CurrentTrackEffect.RemovedFromQueue -> success(removedFromQueue)
        CurrentTrackEffect.ClearedQueue -> success(clearedQueue)
        CurrentTrackEffect.RepeatModeChanged -> success(repeatModeChanged)
        CurrentTrackEffect.QueueShuffled -> success(queueShuffled)
        CurrentTrackEffect.PlayPauseToggled -> success(playbackUpdated)
        CurrentTrackEffect.MoveToIndexFailed -> error(moveToIndexFailed)
        CurrentTrackEffect.LoadCurrentTrackFailed -> error(loadFailed)
        CurrentTrackEffect.SearchFailed -> error(searchFailed)
        CurrentTrackEffect.AddToQueueFailed -> error(addToQueueFailed)
        CurrentTrackEffect.RemoveFromQueueFailed -> error(removeFromQueueFailed)
        CurrentTrackEffect.ClearQueueFailed -> error(clearQueueFailed)
        CurrentTrackEffect.NextTrackFailed -> error(nextTrackFailed)
        CurrentTrackEffect.PreviousTrackFailed -> error(previousTrackFailed)
        CurrentTrackEffect.RepeatModeChangeFailed -> error(repeatModeChangeFailed)
        CurrentTrackEffect.ShuffleQueueFailed -> error(shuffleQueueFailed)
        CurrentTrackEffect.PlayPauseToggleFailed -> error(playPauseToggleFailed)
    }

    private fun success(message: String) = MToastData(message = message, type = MToastType.Success)
    private fun error(message: String) = MToastData(message = message, type = MToastType.Error)
    private fun info(message: String) = MToastData(message = message, type = MToastType.Info)
}

@Composable
internal fun currentTrackEffectMessages() = CurrentTrackEffectMessages(
    commandMessages = currentTrackCommandMessages(),
    addedToQueue = stringResource(Res.string.currenttrack_added_to_queue),
    removedFromQueue = stringResource(Res.string.currenttrack_removed_from_queue),
    clearedQueue = stringResource(Res.string.currenttrack_cleared_queue),
    repeatModeChanged = stringResource(Res.string.currenttrack_repeat_mode_changed),
    queueShuffled = stringResource(Res.string.currenttrack_queue_shuffled),
    playbackUpdated = stringResource(Res.string.currenttrack_playback_updated),
    loadFailed = stringResource(Res.string.currenttrack_load_failed),
    searchFailed = stringResource(Res.string.currenttrack_search_failed),
    addToQueueFailed = stringResource(Res.string.currenttrack_add_to_queue_failed),
    removeFromQueueFailed = stringResource(Res.string.currenttrack_remove_from_queue_failed),
    clearQueueFailed = stringResource(Res.string.currenttrack_clear_queue_failed),
    nextTrackFailed = stringResource(Res.string.currenttrack_next_track_failed),
    previousTrackFailed = stringResource(Res.string.currenttrack_previous_track_failed),
    repeatModeChangeFailed = stringResource(Res.string.currenttrack_repeat_mode_change_failed),
    shuffleQueueFailed = stringResource(Res.string.currenttrack_shuffle_queue_failed),
    playPauseToggleFailed = stringResource(Res.string.currenttrack_play_pause_toggle_failed),
    moveToIndexFailed = stringResource(Res.string.currenttrack_move_to_index_failed),
)

internal data class CurrentTrackCommandMessages(
    val accepted: String,
    private val playbackUpdated: String,
    private val clearedQueue: String,
    private val removedFromQueue: String,
    private val queueShuffled: String,
    private val movedInQueue: String,
    private val repeatModeChanged: String,
    private val playPauseToggleFailed: String,
    private val nextTrackFailed: String,
    private val previousTrackFailed: String,
    private val clearQueueFailed: String,
    private val removeFromQueueFailed: String,
    private val shuffleQueueFailed: String,
    private val moveToIndexFailed: String,
    private val repeatModeChangeFailed: String,
    private val unknownError: String,
    private val userNotInVoiceChannel: String,
    private val playerNotFound: String,
    private val noCurrentTrack: String,
    private val noCurrentOrQueuedTrack: String,
    private val noPreviousTrack: String,
    private val lavalink: String,
    private val queueTooSmall: String,
    private val invalidTrackNumber: String,
    private val invalidTrackIndex: String,
    private val repeatListAlreadyEnabled: String,
    private val repeatAlreadyEnabled: String,
    private val playNotImplemented: String,
    private val discordContextNotFound: String,
    private val interrupted: String,
) {
    fun successMessageFor(commandType: String): String = when (commandType) {
        BotControlCommandTypes.Play,
        BotControlCommandTypes.Pause,
        BotControlCommandTypes.Resume,
        BotControlCommandTypes.Skip,
        BotControlCommandTypes.Previous -> playbackUpdated

        BotControlCommandTypes.Clear -> clearedQueue
        BotControlCommandTypes.Remove -> removedFromQueue
        BotControlCommandTypes.Shuffle -> queueShuffled
        BotControlCommandTypes.MoveToIndex -> movedInQueue
        BotControlCommandTypes.Repeat,
        BotControlCommandTypes.RepeatList -> repeatModeChanged

        else -> playbackUpdated
    }

    fun errorMessageFor(commandType: String, errorKey: String?): String = when (errorKey) {
        "UserNotInVoiceChannel" -> userNotInVoiceChannel
        "PlaybackPlayerNotFound" -> playerNotFound
        "NoCurrentTrack" -> noCurrentTrack
        "NoCurrentOrQueuedTrack" -> noCurrentOrQueuedTrack
        "NoPreviousTrack" -> noPreviousTrack
        "LavalinkError" -> lavalink
        "QueueTooSmall" -> queueTooSmall
        "InvalidTrackNumber" -> invalidTrackNumber
        "InvalidTrackIndex" -> invalidTrackIndex
        "RepeatListAlreadyEnabled" -> repeatListAlreadyEnabled
        "RepeatAlreadyEnabled" -> repeatAlreadyEnabled
        "PlayNotImplemented" -> playNotImplemented
        "DiscordContextNotFound" -> discordContextNotFound
        "Interrupted" -> interrupted
        "UnexpectedError" -> unknownError
        null -> fallbackFailureMessageFor(commandType)
        else -> unknownError
    }

    private fun fallbackFailureMessageFor(commandType: String): String = when (commandType) {
        BotControlCommandTypes.Play,
        BotControlCommandTypes.Pause,
        BotControlCommandTypes.Resume -> playPauseToggleFailed

        BotControlCommandTypes.Skip -> nextTrackFailed
        BotControlCommandTypes.Previous -> previousTrackFailed
        BotControlCommandTypes.Clear -> clearQueueFailed
        BotControlCommandTypes.Remove -> removeFromQueueFailed
        BotControlCommandTypes.Shuffle -> shuffleQueueFailed
        BotControlCommandTypes.MoveToIndex -> moveToIndexFailed
        BotControlCommandTypes.Repeat,
        BotControlCommandTypes.RepeatList -> repeatModeChangeFailed

        else -> unknownError
    }
}

@Composable
internal fun currentTrackCommandMessages() = CurrentTrackCommandMessages(
    accepted = stringResource(Res.string.currenttrack_bot_command_accepted),
    playbackUpdated = stringResource(Res.string.currenttrack_playback_updated),
    clearedQueue = stringResource(Res.string.currenttrack_cleared_queue),
    removedFromQueue = stringResource(Res.string.currenttrack_removed_from_queue),
    queueShuffled = stringResource(Res.string.currenttrack_queue_shuffled),
    movedInQueue = stringResource(Res.string.currenttrack_moved_in_queue),
    repeatModeChanged = stringResource(Res.string.currenttrack_repeat_mode_changed),
    playPauseToggleFailed = stringResource(Res.string.currenttrack_play_pause_toggle_failed),
    nextTrackFailed = stringResource(Res.string.currenttrack_next_track_failed),
    previousTrackFailed = stringResource(Res.string.currenttrack_previous_track_failed),
    clearQueueFailed = stringResource(Res.string.currenttrack_clear_queue_failed),
    removeFromQueueFailed = stringResource(Res.string.currenttrack_remove_from_queue_failed),
    shuffleQueueFailed = stringResource(Res.string.currenttrack_shuffle_queue_failed),
    moveToIndexFailed = stringResource(Res.string.currenttrack_move_to_index_failed),
    repeatModeChangeFailed = stringResource(Res.string.currenttrack_repeat_mode_change_failed),
    unknownError = stringResource(Res.string.currenttrack_bot_command_error_unknown),
    userNotInVoiceChannel = stringResource(Res.string.currenttrack_bot_command_error_user_not_in_voice_channel),
    playerNotFound = stringResource(Res.string.currenttrack_bot_command_error_player_not_found),
    noCurrentTrack = stringResource(Res.string.currenttrack_bot_command_error_no_current_track),
    noCurrentOrQueuedTrack = stringResource(Res.string.currenttrack_bot_command_error_no_current_or_queued_track),
    noPreviousTrack = stringResource(Res.string.currenttrack_bot_command_error_no_previous_track),
    lavalink = stringResource(Res.string.currenttrack_bot_command_error_lavalink),
    queueTooSmall = stringResource(Res.string.currenttrack_bot_command_error_queue_too_small),
    invalidTrackNumber = stringResource(Res.string.currenttrack_bot_command_error_invalid_track_number),
    invalidTrackIndex = stringResource(Res.string.currenttrack_bot_command_error_invalid_track_index),
    repeatListAlreadyEnabled = stringResource(Res.string.currenttrack_bot_command_error_repeat_list_already_enabled),
    repeatAlreadyEnabled = stringResource(Res.string.currenttrack_bot_command_error_repeat_already_enabled),
    playNotImplemented = stringResource(Res.string.currenttrack_bot_command_error_play_not_implemented),
    discordContextNotFound = stringResource(Res.string.currenttrack_bot_command_error_discord_context_not_found),
    interrupted = stringResource(Res.string.currenttrack_bot_command_error_interrupted),
)
