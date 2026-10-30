package com.dc.melodiasmario.feature.currenttrack.presentation.command

import com.dc.melodiasmario.core.domain.currenttrack.usecase.NextTrackUseCase
import com.dc.melodiasmario.core.domain.currenttrack.usecase.PauseUseCase
import com.dc.melodiasmario.core.domain.currenttrack.usecase.PlayUseCase
import com.dc.melodiasmario.core.domain.currenttrack.usecase.PreviousTrackUseCase
import com.dc.melodiasmario.core.domain.currenttrack.usecase.SetRepeatModeUseCase
import com.dc.melodiasmario.core.domain.queue.usecase.AddToQueueUseCase
import com.dc.melodiasmario.core.domain.queue.usecase.ClearQueueUseCase
import com.dc.melodiasmario.core.domain.queue.usecase.MoveTrackToIndexUseCase
import com.dc.melodiasmario.core.domain.queue.usecase.RemoveFromQueueUseCase
import com.dc.melodiasmario.core.domain.queue.usecase.ShuffleQueueUseCase
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.model.currenttrack.RepeatMode
import com.dc.melodiasmario.core.presentation.TrackQueueCommandHandler
import com.dc.melodiasmario.core.presentation.runAction
import com.dc.melodiasmario.core.presentation.runBotCommand
import com.dc.melodiasmario.feature.currenttrack.presentation.CurrentTrackEffect
import com.dc.melodiasmario.feature.currenttrack.presentation.CurrentTrackEvent
import com.dc.melodiasmario.feature.currenttrack.presentation.state.CurrentTrackUiState
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.MutableStateFlow

/**
 * Coordinates current track command operations and their presentation callbacks.
 */
internal class CurrentTrackCommandHandler(
    private val addToQueueUseCase: AddToQueueUseCase,
    private val clearQueueUseCase: ClearQueueUseCase,
    private val nextTrackUseCase: NextTrackUseCase,
    private val previousTrackUseCase: PreviousTrackUseCase,
    private val playUseCase: PlayUseCase,
    private val pauseUseCase: PauseUseCase,
    private val setRepeatModeUseCase: SetRepeatModeUseCase,
    private val removeFromQueueUseCase: RemoveFromQueueUseCase,
    private val moveTrackToIndexUseCase: MoveTrackToIndexUseCase,
    private val shuffleQueueUseCase: ShuffleQueueUseCase,
    private val uiState: MutableStateFlow<CurrentTrackUiState>,
    private val effects: MutableSharedFlow<CurrentTrackEffect>,
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
        acceptedEffect = CurrentTrackEffect.BotCommandAccepted,
        addFailedEffect = CurrentTrackEffect.AddToQueueFailed,
        removeFailedEffect = CurrentTrackEffect.RemoveFromQueueFailed,
        clearFailedEffect = CurrentTrackEffect.ClearQueueFailed,
        shuffleFailedEffect = CurrentTrackEffect.ShuffleQueueFailed,
        moveFailedEffect = CurrentTrackEffect.MoveToIndexFailed,
        currentGuildIdProvider = currentGuildIdProvider,
        onTrackPendingCommand = onTrackPendingCommand,
        onCloseDialog = onCloseDialog,
        onMoveFailed = onMoveFailed,
        clearQueueLoading = ::clearQueueLoading,
        shuffleQueueLoading = ::shuffleQueueLoading
    )

    /**
     * Dispatches the supplied event to the appropriate command operation.
     */
    suspend fun handle(event: CurrentTrackEvent): Unit {
        when (event) {
            CurrentTrackEvent.AddToQueueConfirmed -> trackQueueCommandHandler.addManualInput()
            is CurrentTrackEvent.RemoveFromQueueClicked -> trackQueueCommandHandler.removeFromQueue(
                index = event.index,
                trackId = event.trackId
            )
            CurrentTrackEvent.ClearQueueClicked -> trackQueueCommandHandler.clearQueue()
            CurrentTrackEvent.NextClicked -> nextTrack()
            CurrentTrackEvent.PreviousClicked -> previousTrack()
            CurrentTrackEvent.RepeatClicked -> repeat()
            CurrentTrackEvent.ShuffleClicked -> trackQueueCommandHandler.shuffleQueue()
            CurrentTrackEvent.PlayPauseClicked -> playPause()
            is CurrentTrackEvent.MoveToIndexClicked -> trackQueueCommandHandler.moveTrackToIndex(
                fromIndex = event.fromIndex,
                toIndex = event.toIndex,
                trackId = event.trackId
            )
            CurrentTrackEvent.SelectedSearchResultAddClicked -> trackQueueCommandHandler.addSelectedSearchResultToQueue()
            else -> Unit
        }
    }

    private suspend fun nextTrack() {
        val guildId = currentGuildIdProvider() ?: return
        nextTrackUseCase(guildId = guildId).runBotCommand(
            uiState = uiState,
            effects = effects,
            acceptedEffect = CurrentTrackEffect.BotCommandAccepted,
            failureEffect = CurrentTrackEffect.NextTrackFailed,
            afterAccepted = onTrackPendingCommand
        )
    }

    private suspend fun previousTrack() {
        val guildId = currentGuildIdProvider() ?: return
        previousTrackUseCase(guildId = guildId).runBotCommand(
            uiState = uiState,
            effects = effects,
            acceptedEffect = CurrentTrackEffect.BotCommandAccepted,
            failureEffect = CurrentTrackEffect.PreviousTrackFailed,
            afterAccepted = onTrackPendingCommand
        )
    }

    private suspend fun repeat() {
        val guildId = currentGuildIdProvider() ?: return
        val nextRepeatMode = nextRepeatMode(repeatMode = uiState.value.playback.repeatMode)
        setRepeatModeUseCase(guildId = guildId, repeatMode = nextRepeatMode).runAction(
            uiState = uiState,
            effects = effects,
            successEffect = CurrentTrackEffect.RepeatModeChanged,
            failureEffect = CurrentTrackEffect.RepeatModeChangeFailed
        )
    }

    private suspend fun playPause() {
        val guildId = currentGuildIdProvider() ?: return
        val action = if (uiState.value.playback.isPlaying) {
            pauseUseCase(guildId = guildId)
        } else {
            playUseCase(guildId = guildId)
        }
        action.runBotCommand(
            uiState = uiState,
            effects = effects,
            acceptedEffect = CurrentTrackEffect.BotCommandAccepted,
            failureEffect = CurrentTrackEffect.PlayPauseToggleFailed,
            onLoading = { currentState -> playPauseLoading(currentState = currentState, isLoading = true) },
            onAccepted = { currentState -> playPauseLoading(currentState = currentState, isLoading = false) },
            onError = { currentState, _ -> playPauseLoading(currentState = currentState, isLoading = false) },
            afterAccepted = onTrackPendingCommand
        )
    }

    private fun clearQueueLoading(
        currentState: CurrentTrackUiState,
        isLoading: Boolean
    ): CurrentTrackUiState {
        return currentState.copy(
            actions = currentState.actions.copy(isClearQueueLoading = isLoading)
        )
    }

    private fun shuffleQueueLoading(
        currentState: CurrentTrackUiState,
        isLoading: Boolean
    ): CurrentTrackUiState {
        return currentState.copy(
            actions = currentState.actions.copy(isShuffleQueueLoading = isLoading)
        )
    }

    private fun playPauseLoading(
        currentState: CurrentTrackUiState,
        isLoading: Boolean
    ): CurrentTrackUiState {
        return currentState.copy(
            actions = currentState.actions.copy(isPlayPauseLoading = isLoading)
        )
    }

    private fun nextRepeatMode(repeatMode: RepeatMode) = when (repeatMode) {
        RepeatMode.NONE -> RepeatMode.ONE
        RepeatMode.ONE -> RepeatMode.ALL
        RepeatMode.ALL -> RepeatMode.NONE
    }
}
