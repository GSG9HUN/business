package com.dc.melodiasmario.feature.currenttrack.ui

import androidx.compose.runtime.Composable
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommandTypes
import com.dc.melodiasmario.feature.currenttrack.generated.resources.Res
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
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_track_paused
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_track_resumed
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_track_skipped
import org.jetbrains.compose.resources.stringResource

/**
 * Provides current track command messages for the ui layer.
 *
 * @property accepted Accepted.
 */
internal data class CurrentTrackCommandMessages(
    val accepted: String,
    private val playbackUpdated: String,
    private val clearedQueue: String,
    private val removedFromQueue: String,
    private val queueShuffled: String,
    private val movedInQueue: String,
    private val repeatModeChanged: String,
    private val addedToQueue: String,
    private val trackSkipped: String,
    private val trackPaused: String,
    private val trackResumed: String,
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
    private val interrupted: String
) {
    /**
     * Maps the supplied outcome to its presentation feedback.
     */
    fun successMessageFor(commandType: String): String = when (commandType) {
        BotControlCommandTypes.Play -> addedToQueue
        BotControlCommandTypes.Pause -> trackPaused
        BotControlCommandTypes.Resume -> trackResumed
        BotControlCommandTypes.Skip -> trackSkipped
        BotControlCommandTypes.Previous -> playbackUpdated

        BotControlCommandTypes.Clear -> clearedQueue
        BotControlCommandTypes.Remove -> removedFromQueue
        BotControlCommandTypes.Shuffle -> queueShuffled
        BotControlCommandTypes.MoveToIndex -> movedInQueue
        BotControlCommandTypes.Repeat,
        BotControlCommandTypes.RepeatList -> repeatModeChanged

        else -> playbackUpdated
    }

    /**
     * Maps the supplied outcome to its presentation feedback.
     */
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
        null -> fallbackFailureMessageFor(commandType = commandType)
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

/**
 * Provides current track command messages for the current composition.
 */
@Composable
internal fun currentTrackCommandMessages(): CurrentTrackCommandMessages = CurrentTrackCommandMessages(
    accepted = stringResource(resource = Res.string.currenttrack_bot_command_accepted),
    playbackUpdated = stringResource(resource = Res.string.currenttrack_playback_updated),
    clearedQueue = stringResource(resource = Res.string.currenttrack_cleared_queue),
    removedFromQueue = stringResource(resource = Res.string.currenttrack_removed_from_queue),
    queueShuffled = stringResource(resource = Res.string.currenttrack_queue_shuffled),
    movedInQueue = stringResource(resource = Res.string.currenttrack_moved_in_queue),
    repeatModeChanged = stringResource(resource = Res.string.currenttrack_repeat_mode_changed),
    addedToQueue = stringResource(resource = Res.string.currenttrack_added_to_queue),
    trackSkipped = stringResource(resource = Res.string.currenttrack_track_skipped),
    trackPaused = stringResource(resource = Res.string.currenttrack_track_paused),
    trackResumed = stringResource(resource = Res.string.currenttrack_track_resumed),
    playPauseToggleFailed = stringResource(resource = Res.string.currenttrack_play_pause_toggle_failed),
    nextTrackFailed = stringResource(resource = Res.string.currenttrack_next_track_failed),
    previousTrackFailed = stringResource(resource = Res.string.currenttrack_previous_track_failed),
    clearQueueFailed = stringResource(resource = Res.string.currenttrack_clear_queue_failed),
    removeFromQueueFailed = stringResource(resource = Res.string.currenttrack_remove_from_queue_failed),
    shuffleQueueFailed = stringResource(resource = Res.string.currenttrack_shuffle_queue_failed),
    moveToIndexFailed = stringResource(resource = Res.string.currenttrack_move_to_index_failed),
    repeatModeChangeFailed = stringResource(resource = Res.string.currenttrack_repeat_mode_change_failed),
    unknownError = stringResource(resource = Res.string.currenttrack_bot_command_error_unknown),
    userNotInVoiceChannel = stringResource(resource = Res.string.currenttrack_bot_command_error_user_not_in_voice_channel),
    playerNotFound = stringResource(resource = Res.string.currenttrack_bot_command_error_player_not_found),
    noCurrentTrack = stringResource(resource = Res.string.currenttrack_bot_command_error_no_current_track),
    noCurrentOrQueuedTrack = stringResource(resource = Res.string.currenttrack_bot_command_error_no_current_or_queued_track),
    noPreviousTrack = stringResource(resource = Res.string.currenttrack_bot_command_error_no_previous_track),
    lavalink = stringResource(resource = Res.string.currenttrack_bot_command_error_lavalink),
    queueTooSmall = stringResource(resource = Res.string.currenttrack_bot_command_error_queue_too_small),
    invalidTrackNumber = stringResource(resource = Res.string.currenttrack_bot_command_error_invalid_track_number),
    invalidTrackIndex = stringResource(resource = Res.string.currenttrack_bot_command_error_invalid_track_index),
    repeatListAlreadyEnabled = stringResource(resource = Res.string.currenttrack_bot_command_error_repeat_list_already_enabled),
    repeatAlreadyEnabled = stringResource(resource = Res.string.currenttrack_bot_command_error_repeat_already_enabled),
    playNotImplemented = stringResource(resource = Res.string.currenttrack_bot_command_error_play_not_implemented),
    discordContextNotFound = stringResource(resource = Res.string.currenttrack_bot_command_error_discord_context_not_found),
    interrupted = stringResource(resource = Res.string.currenttrack_bot_command_error_interrupted)
)
