package com.dc.melodiasmario.feature.queue.ui.components

import androidx.compose.runtime.Composable
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommandTypes
import com.dc.melodiasmario.feature.queue.generated.resources.Res
import com.dc.melodiasmario.feature.queue.generated.resources.queue_cleared
import com.dc.melodiasmario.feature.queue.generated.resources.queue_command_accepted
import com.dc.melodiasmario.feature.queue.generated.resources.queue_command_failed
import com.dc.melodiasmario.feature.queue.generated.resources.queue_command_succeeded
import com.dc.melodiasmario.feature.queue.generated.resources.queue_command_timed_out
import com.dc.melodiasmario.feature.queue.generated.resources.queue_invalid_position
import com.dc.melodiasmario.feature.queue.generated.resources.queue_shuffled
import com.dc.melodiasmario.feature.queue.generated.resources.queue_too_small
import com.dc.melodiasmario.feature.queue.generated.resources.queue_track_added
import com.dc.melodiasmario.feature.queue.generated.resources.queue_track_moved
import com.dc.melodiasmario.feature.queue.generated.resources.queue_track_removed
import com.dc.melodiasmario.feature.queue.generated.resources.queue_user_not_in_voice_channel
import org.jetbrains.compose.resources.stringResource

/**
 * Provides queue command messages for the components layer.
 *
 * @property accepted Accepted.
 * @property timedOut Timed out.
 */
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
    private val userNotInVoiceChannel: String
) {
    /**
     * Maps the supplied outcome to its presentation feedback.
     */
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

    /**
     * Maps the supplied outcome to its presentation feedback.
     */
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

/**
 * Provides queue command messages for the current composition.
 */
@Composable
internal fun queueCommandMessages(): QueueCommandMessages = QueueCommandMessages(
    accepted = stringResource(resource = Res.string.queue_command_accepted),
    timedOut = stringResource(resource = Res.string.queue_command_timed_out),
    added = stringResource(resource = Res.string.queue_track_added),
    removed = stringResource(resource = Res.string.queue_track_removed),
    cleared = stringResource(resource = Res.string.queue_cleared),
    shuffled = stringResource(resource = Res.string.queue_shuffled),
    moved = stringResource(resource = Res.string.queue_track_moved),
    succeeded = stringResource(resource = Res.string.queue_command_succeeded),
    failed = stringResource(resource = Res.string.queue_command_failed),
    invalidPosition = stringResource(resource = Res.string.queue_invalid_position),
    queueTooSmall = stringResource(resource = Res.string.queue_too_small),
    userNotInVoiceChannel =
        stringResource(resource = Res.string.queue_user_not_in_voice_channel)
)
