package com.dc.melodiasmario.feature.currenttrack.presentation.command

import com.dc.melodiasmario.core.presentation.runAction
import com.dc.melodiasmario.core.presentation.runBotCommand
import com.dc.melodiasmario.core.presentation.TrackQueueCommandHandler
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
import com.dc.melodiasmario.feature.currenttrack.presentation.CurrentTrackEffect
import com.dc.melodiasmario.feature.currenttrack.presentation.CurrentTrackEvent
import com.dc.melodiasmario.feature.currenttrack.presentation.state.CurrentTrackUiState
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.MutableStateFlow

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
    private val state: MutableStateFlow<CurrentTrackUiState>,
    private val effects: MutableSharedFlow<CurrentTrackEffect>,
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
        shuffleQueueLoading = ::shuffleQueueLoading,
    )

    suspend fun handle(event: CurrentTrackEvent) {
        when (event) {
            CurrentTrackEvent.AddToQueueConfirmed -> queueCommands.addManualInput()
            is CurrentTrackEvent.RemoveFromQueueClicked -> queueCommands.removeFromQueue(event.index, event.trackId)
            CurrentTrackEvent.ClearQueueClicked -> queueCommands.clearQueue()
            CurrentTrackEvent.NextClicked -> nextTrack()
            CurrentTrackEvent.PreviousClicked -> previousTrack()
            CurrentTrackEvent.RepeatClicked -> repeat()
            CurrentTrackEvent.ShuffleClicked -> queueCommands.shuffleQueue()
            CurrentTrackEvent.PlayPauseClicked -> playPause()
            is CurrentTrackEvent.MoveToIndexClicked -> queueCommands.moveTrackToIndex(
                fromIndex = event.fromIndex,
                toIndex = event.toIndex,
                trackId = event.trackId
            )
            CurrentTrackEvent.SelectedSearchResultAddClicked -> queueCommands.addSelectedSearchResultToQueue()
            else -> Unit
        }
    }

    private suspend fun nextTrack() {
        val guildId = currentGuildIdProvider() ?: return
        nextTrackUseCase(guildId).runBotCommand(
            state = state,
            effects = effects,
            acceptedEffect = CurrentTrackEffect.BotCommandAccepted,
            failureEffect = CurrentTrackEffect.NextTrackFailed,
            afterAccepted = onTrackPendingCommand,
        )
    }

    private suspend fun previousTrack() {
        val guildId = currentGuildIdProvider() ?: return
        previousTrackUseCase(guildId).runBotCommand(
            state = state,
            effects = effects,
            acceptedEffect = CurrentTrackEffect.BotCommandAccepted,
            failureEffect = CurrentTrackEffect.PreviousTrackFailed,
            afterAccepted = onTrackPendingCommand,
        )
    }

    private suspend fun repeat() {
        val guildId = currentGuildIdProvider() ?: return
        val nextRepeatMode = nextRepeatMode(state.value.playback.repeatMode)
        setRepeatModeUseCase(guildId, nextRepeatMode).runAction(
            state = state,
            effects = effects,
            successEffect = CurrentTrackEffect.RepeatModeChanged,
            failureEffect = CurrentTrackEffect.RepeatModeChangeFailed,
        )
    }

    private suspend fun playPause() {
        val guildId = currentGuildIdProvider() ?: return
        val action = if (state.value.playback.isPlaying) {
            pauseUseCase(guildId)
        } else {
            playUseCase(guildId)
        }
        action.runBotCommand(
            state = state,
            effects = effects,
            acceptedEffect = CurrentTrackEffect.BotCommandAccepted,
            failureEffect = CurrentTrackEffect.PlayPauseToggleFailed,
            onLoading = { currentState -> playPauseLoading(currentState, true) },
            onAccepted = { currentState -> playPauseLoading(currentState, false) },
            onError = { currentState, _ -> playPauseLoading(currentState, false) },
            afterAccepted = onTrackPendingCommand,
        )
    }

    private fun clearQueueLoading(
        currentState: CurrentTrackUiState,
        isLoading: Boolean,
    ): CurrentTrackUiState {
        return currentState.copy(
            actions = currentState.actions.copy(isClearQueueLoading = isLoading)
        )
    }

    private fun shuffleQueueLoading(
        currentState: CurrentTrackUiState,
        isLoading: Boolean,
    ): CurrentTrackUiState {
        return currentState.copy(
            actions = currentState.actions.copy(isShuffleQueueLoading = isLoading)
        )
    }

    private fun playPauseLoading(
        currentState: CurrentTrackUiState,
        isLoading: Boolean,
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