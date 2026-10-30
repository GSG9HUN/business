package com.dc.melodiasmario.core.presentation

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.model.currenttrack.Track
import com.dc.melodiasmario.core.presentation.music.search.MusicSearchUiState
import com.dc.melodiasmario.core.presentation.queue.TrackQueueReducer
import com.dc.melodiasmario.core.presentation.queue.TrackQueueUiState
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.onEach
import kotlinx.coroutines.flow.update

/**
 * Coordinates track queue command operations and their presentation callbacks.
 */
class TrackQueueCommandHandler<State, Effect>(
    private val addToQueue: (guildId: String, query: String) -> Flow<Resource<BotControlCommand>>,
    private val removeFromQueue: (guildId: String, trackNumber: String) -> Flow<Resource<BotControlCommand>>,
    private val clearQueue: (guildId: String) -> Flow<Resource<BotControlCommand>>,
    private val shuffleQueue: (guildId: String) -> Flow<Resource<BotControlCommand>>,
    private val moveTrackToIndex: (guildId: String, fromIndex: Int, toIndex: Int) -> Flow<Resource<BotControlCommand>>,
    private val uiState: MutableStateFlow<State>,
    private val effects: MutableSharedFlow<Effect>,
    private val acceptedEffect: Effect,
    private val addFailedEffect: Effect,
    private val removeFailedEffect: Effect,
    private val clearFailedEffect: Effect,
    private val shuffleFailedEffect: Effect,
    private val moveFailedEffect: Effect,
    private val currentGuildIdProvider: () -> String?,
    private val onTrackPendingCommand: suspend (command: BotControlCommand) -> Unit,
    private val onCloseDialog: () -> Unit,
    private val onMoveFailed: () -> Unit,
    private val clearQueueLoading: (uiState: State, loading: Boolean) -> State = { value, _ -> value },
    private val shuffleQueueLoading: (uiState: State, loading: Boolean) -> State = { value, _ -> value }
) where State : MusicSearchUiState<State>,
        State : TrackQueueUiState<Track, State>,
        Effect : MviEffect {
    private val trackQueueReducer = TrackQueueReducer()

    /**
     * Validates the manual draft and submits it to the selected guild queue.
     */
    suspend fun addManualInput(): Unit {
        val sheet = uiState.value.addMusic

        if (!sheet.canSubmitManual) {
            return
        }

        enqueueQuery(query = sheet.manualDraft.trim())
    }

    /**
     * Submits the selected enqueueable search result to the guild queue.
     */
    suspend fun addSelectedSearchResultToQueue(): Unit {
        val sheet = uiState.value.addMusic
        val result = sheet.results.firstOrNull { searchResult -> searchResult.id == sheet.selectedResultId } ?: return
        val url = result.canonicalUrl?.takeIf { textValue -> textValue.isNotBlank() } ?: return

        if (!result.canEnqueue) {
            return
        }

        enqueueQuery(query = url)
    }

    private suspend fun enqueueQuery(query: String) {
        val guildId = currentGuildIdProvider() ?: return

        if (query.isBlank()) {
            return
        }

        addToQueue(guildId, query).runBotCommand(
            uiState = uiState,
            effects = effects,
            acceptedEffect = acceptedEffect,
            failureEffect = addFailedEffect,
            onLoading = { currentUiState -> addLoading(currentUiState = currentUiState, loading = true) },
            onAccepted = { currentUiState -> addLoading(currentUiState = currentUiState, loading = false) },
            onError = { value, _ -> addLoading(currentUiState = value, loading = false) },
            afterAccepted = { command ->
                onCloseDialog()
                onTrackPendingCommand(command)
            }
        )
    }

    /**
     * Requests removal of a track from the playback queue.
     */
    suspend fun removeFromQueue(index: Int, trackId: String? = null): Unit {
        val guildId = currentGuildIdProvider() ?: return
        val track = uiState.value.tracks.getOrNull(index = index) ?: return

        if (trackId != null && track.id != trackId) {
            return
        }

        removeFromQueue(guildId, (index + 1).toString()).runBotCommand(
            uiState = uiState,
            effects = effects,
            acceptedEffect = acceptedEffect,
            failureEffect = removeFailedEffect,
            afterAccepted = onTrackPendingCommand
        )
    }

    /**
     * Requests moving a queued track between zero-based queue indexes.
     */
    suspend fun moveTrackToIndex(fromIndex: Int, toIndex: Int, trackId: String? = null): Unit {
        val guildId = currentGuildIdProvider() ?: return
        val tracks = uiState.value.tracks
        val track = tracks.getOrNull(index = fromIndex) ?: return

        if (fromIndex == toIndex || toIndex !in tracks.indices) {
            return
        }

        if (trackId != null && track.id != trackId) {
            return
        }

        uiState.update { currentUiState -> trackQueueReducer.reorder(uiState = currentUiState, fromIndex = fromIndex, toIndex = toIndex) }
        moveTrackToIndex(guildId, fromIndex, toIndex).onEach { result ->

            if (result is Resource.Error) {
                onMoveFailed()
            }
        }
            .runBotCommand(
                uiState = uiState,
                effects = effects,
                acceptedEffect = acceptedEffect,
                failureEffect = moveFailedEffect,
                afterAccepted = onTrackPendingCommand
            )
    }

    /**
     * Requests clearing the guild playback queue.
     */
    suspend fun clearQueue(): Unit {
        val guildId = currentGuildIdProvider() ?: return

        if (uiState.value.tracks.isEmpty()) {
            return
        }

        onCloseDialog()
        clearQueue(guildId).runBotCommand(
            uiState = uiState,
            effects = effects,
            acceptedEffect = acceptedEffect,
            failureEffect = clearFailedEffect,
            onLoading = { currentUiState -> clearQueueLoading(currentUiState, true) },
            onAccepted = { currentUiState -> clearQueueLoading(currentUiState, false) },
            onError = { value, _ -> clearQueueLoading(value, false) },
            afterAccepted = onTrackPendingCommand
        )
    }

    /**
     * Requests shuffling the guild playback queue.
     */
    suspend fun shuffleQueue(): Unit {
        val guildId = currentGuildIdProvider() ?: return

        if (uiState.value.tracks.size < 2) {
            return
        }

        onCloseDialog()
        shuffleQueue(guildId).runBotCommand(
            uiState = uiState,
            effects = effects,
            acceptedEffect = acceptedEffect,
            failureEffect = shuffleFailedEffect,
            onLoading = { currentUiState -> shuffleQueueLoading(currentUiState, true) },
            onAccepted = { currentUiState -> shuffleQueueLoading(currentUiState, false) },
            onError = { value, _ -> shuffleQueueLoading(value, false) },
            afterAccepted = onTrackPendingCommand
        )
    }

    private fun addLoading(currentUiState: State, loading: Boolean): State {
        val addMusic = currentUiState.addMusic.copy(isManualLoading = loading)
        return currentUiState.withAddMusic(addMusic = addMusic)
    }
}
