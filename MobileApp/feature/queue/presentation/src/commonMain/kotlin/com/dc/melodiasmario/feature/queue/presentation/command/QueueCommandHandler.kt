package com.dc.melodiasmario.feature.queue.presentation.command

import com.dc.melodiasmario.core.domain.queue.usecase.AddToQueueUseCase
import com.dc.melodiasmario.core.domain.queue.usecase.ClearQueueUseCase
import com.dc.melodiasmario.core.domain.queue.usecase.MoveTrackToIndexUseCase
import com.dc.melodiasmario.core.domain.queue.usecase.RemoveFromQueueUseCase
import com.dc.melodiasmario.core.domain.queue.usecase.ShuffleQueueUseCase
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.presentation.TrackQueueCommandHandler
import com.dc.melodiasmario.feature.queue.presentation.QueueEffect
import com.dc.melodiasmario.feature.queue.presentation.QueueEvent
import com.dc.melodiasmario.feature.queue.presentation.QueueUiState
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.MutableStateFlow

/**
 * Coordinates queue command operations and their presentation callbacks.
 */
internal class QueueCommandHandler(
    private val addToQueueUseCase: AddToQueueUseCase,
    private val removeFromQueueUseCase: RemoveFromQueueUseCase,
    private val clearQueueUseCase: ClearQueueUseCase,
    private val shuffleQueueUseCase: ShuffleQueueUseCase,
    private val moveTrackToIndexUseCase: MoveTrackToIndexUseCase,
    private val uiState: MutableStateFlow<QueueUiState>,
    private val effects: MutableSharedFlow<QueueEffect>,
    private val currentGuildIdProvider: () -> String?,
    private val onTrackPendingCommand: suspend (command: BotControlCommand) -> Unit,
    private val onCloseDialog: () -> Unit,
    onMoveFailed: () -> Unit
) {

    private val trackQueueCommandHandler = TrackQueueCommandHandler(
        addToQueue = addToQueueUseCase::invoke,
        removeFromQueue = removeFromQueueUseCase::invoke,
        clearQueue = clearQueueUseCase::invoke,
        shuffleQueue = shuffleQueueUseCase::invoke,
        moveTrackToIndex = moveTrackToIndexUseCase::invoke,
        uiState = uiState,
        effects = effects,
        acceptedEffect = QueueEffect.BotCommandAccepted,
        addFailedEffect = QueueEffect.AddToQueueFailed,
        removeFailedEffect = QueueEffect.RemoveFromQueueFailed,
        clearFailedEffect = QueueEffect.ClearQueueFailed,
        shuffleFailedEffect = QueueEffect.ShuffleQueueFailed,
        moveFailedEffect = QueueEffect.MoveTrackFailed,
        currentGuildIdProvider = currentGuildIdProvider,
        onTrackPendingCommand = onTrackPendingCommand,
        onCloseDialog = onCloseDialog,
        onMoveFailed = onMoveFailed
    )

    /**
     * Dispatches the supplied event to the appropriate command operation.
     */
    suspend fun handle(event: QueueEvent): Unit {
        when (event) {
            QueueEvent.ManualAddClicked -> trackQueueCommandHandler.addManualInput()
            QueueEvent.SelectedSearchResultAddClicked -> trackQueueCommandHandler.addSelectedSearchResultToQueue()
            is QueueEvent.RemoveTrackClicked -> trackQueueCommandHandler.removeFromQueue(
                index = event.index,
                trackId = event.trackId
            )

            is QueueEvent.MoveTrackClicked -> trackQueueCommandHandler.moveTrackToIndex(
                fromIndex = event.fromIndex,
                toIndex = event.toIndex,
                trackId = event.trackId
            )

            QueueEvent.ClearQueueClicked -> trackQueueCommandHandler.clearQueue()
            QueueEvent.ShuffleQueueClicked -> trackQueueCommandHandler.shuffleQueue()
            else -> Unit
        }
    }
}
