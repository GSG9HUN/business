package com.dc.melodiasmario.core.presentation

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.presentation.queue.TrackQueueReducer
import com.dc.melodiasmario.core.presentation.queue.TrackQueueUiState
import com.dc.melodiasmario.core.presentation.music.search.MusicSearchUiState
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.onEach
import kotlinx.coroutines.flow.update
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.model.currenttrack.Track

class TrackQueueCommandHandler<State, Effect>(
    private val addToQueue: (guildId: String, query: String) -> Flow<Resource<BotControlCommand>>,
    private val removeFromQueue: (String, String) -> Flow<Resource<BotControlCommand>>,
    private val clearQueue: (String) -> Flow<Resource<BotControlCommand>>,
    private val shuffleQueue: (String) -> Flow<Resource<BotControlCommand>>,
    private val moveTrackToIndex: (String, Int, Int) -> Flow<Resource<BotControlCommand>>,
    private val state: MutableStateFlow<State>,
    private val effects: MutableSharedFlow<Effect>,
    private val acceptedEffect: Effect,
    private val addFailedEffect: Effect,
    private val removeFailedEffect: Effect,
    private val clearFailedEffect: Effect,
    private val shuffleFailedEffect: Effect,
    private val moveFailedEffect: Effect,
    private val currentGuildIdProvider: () -> String?,
    private val onTrackPendingCommand: suspend (BotControlCommand) -> Unit,
    private val onCloseDialog: () -> Unit,
    private val onMoveFailed: () -> Unit,
    private val clearQueueLoading: (State, Boolean) -> State = { value, _ -> value },
    private val shuffleQueueLoading: (State, Boolean) -> State = { value, _ -> value }
) where State : MusicSearchUiState<State>,
        State : TrackQueueUiState<Track, State>,
        Effect : MviEffect {
    private val reducer = TrackQueueReducer()

    suspend fun addManualInput() {
        val sheet = state.value.addMusic
        if (!sheet.canSubmitManual) {
            return
        }
        enqueueQuery(query = sheet.manualDraft.trim())
    }

    suspend fun addSelectedSearchResultToQueue() {
        val sheet = state.value.addMusic
        val result = sheet.results.firstOrNull { it.id == sheet.selectedResultId } ?: return
        val url = result.canonicalUrl?.takeIf { it.isNotBlank() } ?: return
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
            state = state,
            effects = effects,
            acceptedEffect = acceptedEffect,
            failureEffect = addFailedEffect,
            onLoading = { addLoading(value = it, loading = true) },
            onAccepted = { addLoading(value = it, loading = false) },
            onError = { value, _ -> addLoading(value = value, loading = false) },
            afterAccepted = { command ->
                onCloseDialog()
                onTrackPendingCommand(command)
            }
        )
    }

    suspend fun removeFromQueue(index: Int, trackId: String? = null) {
        val guildId = currentGuildIdProvider() ?: return
        val track = state.value.tracks.getOrNull(index) ?: return
        if (trackId != null && track.id != trackId) {
            return
        }
        removeFromQueue(guildId, (index + 1).toString()).runBotCommand(
            state = state,
            effects = effects,
            acceptedEffect = acceptedEffect,
            failureEffect = removeFailedEffect,
            afterAccepted = onTrackPendingCommand
        )
    }

    suspend fun moveTrackToIndex(fromIndex: Int, toIndex: Int, trackId: String? = null) {
        val guildId = currentGuildIdProvider() ?: return
        val tracks = state.value.tracks
        val track = tracks.getOrNull(fromIndex) ?: return
        if (fromIndex == toIndex || toIndex !in tracks.indices) {
            return
        }
        if (trackId != null && track.id != trackId) {
            return
        }
        state.update { reducer.reorder(it, fromIndex, toIndex) }
        moveTrackToIndex(guildId, fromIndex, toIndex).onEach { result ->
            if (result is Resource.Error) {
                onMoveFailed()
            }
        }.runBotCommand(
            state = state,
            effects = effects,
            acceptedEffect = acceptedEffect,
            failureEffect = moveFailedEffect,
            afterAccepted = onTrackPendingCommand
        )
    }

    suspend fun clearQueue() {
        val guildId = currentGuildIdProvider() ?: return
        if (state.value.tracks.isEmpty()) {
            return
        }
        onCloseDialog()
        clearQueue(guildId).runBotCommand(
            state = state,
            effects = effects,
            acceptedEffect = acceptedEffect,
            failureEffect = clearFailedEffect,
            onLoading = { clearQueueLoading(it, true) },
            onAccepted = { clearQueueLoading(it, false) },
            onError = { value, _ -> clearQueueLoading(value, false) },
            afterAccepted = onTrackPendingCommand
        )
    }

    suspend fun shuffleQueue() {
        val guildId = currentGuildIdProvider() ?: return
        if (state.value.tracks.size < 2) {
            return
        }
        onCloseDialog()
        shuffleQueue(guildId).runBotCommand(
            state = state,
            effects = effects,
            acceptedEffect = acceptedEffect,
            failureEffect = shuffleFailedEffect,
            onLoading = { shuffleQueueLoading(it, true) },
            onAccepted = { shuffleQueueLoading(it, false) },
            onError = { value, _ -> shuffleQueueLoading(value, false) },
            afterAccepted = onTrackPendingCommand
        )
    }

    private fun addLoading(value: State, loading: Boolean): State {
        val addMusic = value.addMusic.copy(isManualLoading = loading)
        return value.withAddMusic(addMusic = addMusic)
    }
}