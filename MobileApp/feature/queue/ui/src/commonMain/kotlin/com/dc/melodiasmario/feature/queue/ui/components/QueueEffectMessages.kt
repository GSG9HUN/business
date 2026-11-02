package com.dc.melodiasmario.feature.queue.ui.components

import androidx.compose.runtime.Composable
import com.dc.melodiasmario.core.commonui.feedback.model.MToastData
import com.dc.melodiasmario.core.commonui.feedback.model.MToastType
import com.dc.melodiasmario.feature.queue.generated.resources.Res
import com.dc.melodiasmario.feature.queue.generated.resources.queue_command_failed
import com.dc.melodiasmario.feature.queue.generated.resources.queue_playlist_save_failed
import com.dc.melodiasmario.feature.queue.generated.resources.queue_search_failed
import com.dc.melodiasmario.feature.queue.presentation.QueueEffect
import org.jetbrains.compose.resources.stringResource

/**
 * Provides queue effect messages for the components layer.
 */
internal data class QueueEffectMessages(
    private val commands: QueueCommandMessages,
    private val operationFailed: String,
    private val searchFailed: String,
    private val playlistSaveFailed: String
) {
    /**
     * Maps the supplied effect to localized toast data, or returns null when no toast is needed.
     */
    fun toastDataFor(effect: QueueEffect): MToastData? {
        return when (effect) {
            QueueEffect.NavigateToGuildSelector -> null

            QueueEffect.BotCommandAccepted -> MToastData(
                message = commands.accepted,
                type = MToastType.Info
            )

            is QueueEffect.BotCommandSucceeded -> MToastData(
                message = commands.successMessageFor(commandType = effect.commandType),
                type = MToastType.Success
            )

            is QueueEffect.BotCommandFailed -> MToastData(
                message = commands.errorMessageFor(errorKey = effect.errorKey),
                type = MToastType.Error
            )

            QueueEffect.BotCommandTimedOut -> MToastData(
                message = commands.timedOut,
                type = MToastType.Warning,
                durationMillis = 7_000
            )

            QueueEffect.SearchFailed -> MToastData(
                message = searchFailed,
                type = MToastType.Error
            )

            QueueEffect.AddToQueueFailed,
            QueueEffect.RemoveFromQueueFailed,
            QueueEffect.ClearQueueFailed,
            QueueEffect.ShuffleQueueFailed,
            QueueEffect.MoveTrackFailed -> MToastData(
                message = operationFailed,
                type = MToastType.Error
            )

            is QueueEffect.PlaylistSaveFailed -> MToastData(
                message = listOfNotNull(
                    playlistSaveFailed,
                    effect.detail?.takeIf { textValue -> textValue.isNotBlank() }
                ).joinToString(separator = "\n"),
                type = MToastType.Error,
                durationMillis = 10_000
            )
        }
    }
}

/**
 * Provides queue effect messages for the current composition.
 */
@Composable
internal fun queueEffectMessages(): QueueEffectMessages = QueueEffectMessages(
    commands = queueCommandMessages(),
    operationFailed = stringResource(resource = Res.string.queue_command_failed),
    searchFailed = stringResource(resource = Res.string.queue_search_failed),
    playlistSaveFailed = stringResource(resource = Res.string.queue_playlist_save_failed)
)
