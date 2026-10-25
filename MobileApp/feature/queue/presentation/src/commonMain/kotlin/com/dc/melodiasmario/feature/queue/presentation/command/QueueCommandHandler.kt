package com.dc.melodiasmario.feature.queue.presentation.command

import com.dc.melodiasmario.core.presentation.TrackQueueCommandHandler
import com.dc.melodiasmario.core.domain.queue.usecase.AddToQueueUseCase
import com.dc.melodiasmario.core.domain.queue.usecase.ClearQueueUseCase
import com.dc.melodiasmario.core.domain.queue.usecase.MoveTrackToIndexUseCase
import com.dc.melodiasmario.core.domain.queue.usecase.RemoveFromQueueUseCase
import com.dc.melodiasmario.core.domain.queue.usecase.ShuffleQueueUseCase
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.feature.queue.presentation.QueueEffect
import com.dc.melodiasmario.feature.queue.presentation.QueueEvent
import com.dc.melodiasmario.feature.queue.presentation.QueueUiState
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.MutableStateFlow

internal class QueueCommandHandler(
    private val addToQueueUseCase: AddToQueueUseCase,
    private val removeFromQueueUseCase: RemoveFromQueueUseCase,
    private val clearQueueUseCase: ClearQueueUseCase,
    private val shuffleQueueUseCase: ShuffleQueueUseCase,
    private val moveTrackToIndexUseCase: MoveTrackToIndexUseCase,
    private val state: MutableStateFlow<QueueUiState>,
    private val effects: MutableSharedFlow<QueueEffect>,
    private val currentGuildIdProvider: () -> String?,
    private val onTrackPendingCommand: suspend (BotControlCommand) -> Unit,
    private val onCloseDialog: () -> Unit,
    onMoveFailed: () -> Unit,
) {

    private val queueCommands = TrackQueueCommandHandler(
        addToQueue = addToQueueUseCase::invoke,
        removeFromQueue = removeFromQueueUseCase::invoke,
        clearQueue = clearQueueUseCase::invoke,
        shuffleQueue = shuffleQueueUseCase::invoke,
        moveTrackToIndex = moveTrackToIndexUseCase::invoke,
        state = state,
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
        onMoveFailed = onMoveFailed,
    )

    suspend fun handle(event: QueueEvent) {
        when (event) {
            QueueEvent.ManualAddClicked -> queueCommands.addManualInput()
            QueueEvent.SelectedSearchResultAddClicked -> queueCommands.addSelectedSearchResultToQueue()
            is QueueEvent.RemoveTrackClicked -> queueCommands.removeFromQueue(
                event.index,
                event.trackId
            )

            is QueueEvent.MoveTrackClicked -> queueCommands.moveTrackToIndex(
                event.fromIndex,
                event.toIndex,
                event.trackId
            )

            QueueEvent.ClearQueueClicked -> queueCommands.clearQueue()
            QueueEvent.ShuffleQueueClicked -> queueCommands.shuffleQueue()
            else -> Unit
        }
    }
}