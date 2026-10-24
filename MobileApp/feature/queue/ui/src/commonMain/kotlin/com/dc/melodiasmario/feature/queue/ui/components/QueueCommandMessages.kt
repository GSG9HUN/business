package com.dc.melodiasmario.feature.queue.ui.components

import androidx.compose.runtime.Composable
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommandTypes
import com.dc.melodiasmario.feature.queue.generated.resources.*
import org.jetbrains.compose.resources.stringResource

internal data class QueueCommandMessages(
    val accepted: String,
    val timedOut: String,
    private val added: String,
    private val removed: String,
    private val cleared: String,
    private val shuffled: String,
    private val moved: String,
    private val succeeded: String,
    private val failed: String,
    private val invalidPosition: String,
    private val queueTooSmall: String,
    private val userNotInVoiceChannel: String,
) {
    fun successMessageFor(commandType: String): String {
        return when (commandType) {
            BotControlCommandTypes.Play -> added
            BotControlCommandTypes.Remove -> removed
            BotControlCommandTypes.Clear -> cleared
            BotControlCommandTypes.Shuffle -> shuffled
            BotControlCommandTypes.MoveToIndex -> moved
            else -> succeeded
        }
    }

    fun errorMessageFor(errorKey: String?): String {
        return when (errorKey) {
            "InvalidTrackNumber",
            "InvalidTrackIndex" -> invalidPosition

            "QueueTooSmall" -> queueTooSmall

            "UserNotInVoiceChannel" -> userNotInVoiceChannel

            else -> failed
        }
    }
}

@Composable
internal fun queueCommandMessages() = QueueCommandMessages(
    accepted = stringResource(Res.string.queue_command_accepted),
    timedOut = stringResource(Res.string.queue_command_timed_out),
    added = stringResource(Res.string.queue_track_added),
    removed = stringResource(Res.string.queue_track_removed),
    cleared = stringResource(Res.string.queue_cleared),
    shuffled = stringResource(Res.string.queue_shuffled),
    moved = stringResource(Res.string.queue_track_moved),
    succeeded = stringResource(Res.string.queue_command_succeeded),
    failed = stringResource(Res.string.queue_command_failed),
    invalidPosition = stringResource(Res.string.queue_invalid_position),
    queueTooSmall = stringResource(Res.string.queue_too_small),
    userNotInVoiceChannel =
        stringResource(Res.string.queue_user_not_in_voice_channel),
)