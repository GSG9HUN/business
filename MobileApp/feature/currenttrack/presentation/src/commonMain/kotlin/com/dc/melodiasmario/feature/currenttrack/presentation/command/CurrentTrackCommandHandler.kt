package com.dc.melodiasmario.feature.currenttrack.presentation.command

import com.dc.melodiasmario.core.common.presentation.runAction
import com.dc.melodiasmario.core.common.presentation.runBotCommand
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
import com.dc.melodiasmario.feature.currenttrack.presentation.queue.CurrentTrackQueueReducer
import com.dc.melodiasmario.feature.currenttrack.presentation.state.CurrentTrackUiState
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.update

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
    private val onTrackPendingCommand: (BotControlCommand) -> Unit,
    private val onCloseDialog: () -> Unit,
) {
    suspend fun handle(event: CurrentTrackEvent) {
        when (event) {
            CurrentTrackEvent.AddToQueueConfirmed -> addToQueue()
            is CurrentTrackEvent.RemoveFromQueueClicked -> removeFromQueue(event.index)
            CurrentTrackEvent.ClearQueueClicked -> clearQueue()
            CurrentTrackEvent.NextClicked -> nextTrack()
            CurrentTrackEvent.PreviousClicked -> previousTrack()
            CurrentTrackEvent.RepeatClicked -> repeat()
            CurrentTrackEvent.ShuffleClicked -> shuffleQueue()
            CurrentTrackEvent.PlayPauseClicked -> playPause()
            is CurrentTrackEvent.MoveToIndexClicked -> moveTrackToIndex(
                fromIndex = event.fromIndex,
                toIndex = event.toIndex,
            )
            CurrentTrackEvent.SelectedSearchResultAddClicked -> addSelectedSearchResultToQueue()
            else -> Unit
        }
    }

    private suspend fun moveTrackToIndex(fromIndex: Int, toIndex: Int) {
        val guildId = currentGuildIdProvider() ?: return
        if (fromIndex == toIndex) return

        state.update { CurrentTrackQueueReducer.reorder(it, fromIndex, toIndex) }

        moveTrackToIndexUseCase(guildId, fromIndex, toIndex).runBotCommand(
            state = state,
            effects = effects,
            acceptedEffect = CurrentTrackEffect.BotCommandAccepted,
            failureEffect = CurrentTrackEffect.MoveToIndexFailed,
            afterAccepted = onTrackPendingCommand,
        )
    }

    private suspend fun addToQueue() {
        val urlOrQuery = state.value.addMusic.manualDraft.trim()
        if (!state.value.addMusic.canSubmitManual) return

        enqueueQuery(urlOrQuery, closeAfterAccepted = true)
    }

    private suspend fun addSelectedSearchResultToQueue() {
        val addMusic = state.value.addMusic
        val result = addMusic.results.firstOrNull { it.id == addMusic.selectedResultId } ?: return
        val canonicalUrl = result.canonicalUrl ?: return

        enqueueQuery(canonicalUrl, closeAfterAccepted = true)
    }

    private suspend fun enqueueQuery(
        query: String,
        closeAfterAccepted: Boolean,
    ) {
        val guildId = currentGuildIdProvider() ?: return
        if (query.isBlank()) return

        addToQueueUseCase(guildId, query).runBotCommand(
            state = state,
            effects = effects,
            acceptedEffect = CurrentTrackEffect.BotCommandAccepted,
            failureEffect = CurrentTrackEffect.AddToQueueFailed,
            onLoading = { currentState -> addToQueueLoading(currentState, true) },
            onAccepted = { currentState -> addToQueueLoading(currentState, false) },
            onError = { currentState, _ -> addToQueueLoading(currentState, false) },
            afterAccepted = { command ->
                onTrackPendingCommand(command)
                if (closeAfterAccepted) {
                    onCloseDialog()
                }
            },
        )
    }

    private suspend fun clearQueue() {
        val guildId = currentGuildIdProvider() ?: return
        onCloseDialog()
        clearQueueUseCase(guildId).runBotCommand(
            state = state,
            effects = effects,
            acceptedEffect = CurrentTrackEffect.BotCommandAccepted,
            failureEffect = CurrentTrackEffect.ClearQueueFailed,
            onLoading = { currentState -> clearQueueLoading(currentState, true) },
            onAccepted = { currentState -> clearQueueLoading(currentState, false) },
            onError = { currentState, _ -> clearQueueLoading(currentState, false) },
            afterAccepted = onTrackPendingCommand,
        )
    }

    private suspend fun removeFromQueue(index: Int) {
        val guildId = currentGuildIdProvider() ?: return
        val trackNumber = (index + 1).toString()
        removeFromQueueUseCase(guildId, trackNumber).runBotCommand(
            state = state,
            effects = effects,
            acceptedEffect = CurrentTrackEffect.BotCommandAccepted,
            failureEffect = CurrentTrackEffect.RemoveFromQueueFailed,
            afterAccepted = onTrackPendingCommand,
        )
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

    private suspend fun shuffleQueue() {
        val guildId = currentGuildIdProvider() ?: return
        onCloseDialog()
        shuffleQueueUseCase(guildId).runBotCommand(
            state = state,
            effects = effects,
            acceptedEffect = CurrentTrackEffect.BotCommandAccepted,
            failureEffect = CurrentTrackEffect.ShuffleQueueFailed,
            onLoading = { currentState -> shuffleQueueLoading(currentState, true) },
            onAccepted = { currentState -> shuffleQueueLoading(currentState, false) },
            onError = { currentState, _ -> shuffleQueueLoading(currentState, false) },
            afterAccepted = onTrackPendingCommand,
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

    private fun addToQueueLoading(
        currentState: CurrentTrackUiState,
        isLoading: Boolean,
    ): CurrentTrackUiState {
        return currentState.copy(
            addMusic = currentState.addMusic.copy(isManualLoading = isLoading)
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
