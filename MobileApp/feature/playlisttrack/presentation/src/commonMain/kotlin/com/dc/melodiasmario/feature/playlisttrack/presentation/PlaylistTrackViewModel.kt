package com.dc.melodiasmario.feature.playlisttrack.presentation

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.commonui.music.validation.canSubmitMusicInput
import com.dc.melodiasmario.core.domain.playlisttrack.usecase.AddPlaylistTrackUseCase
import com.dc.melodiasmario.core.domain.playlisttrack.usecase.GetPlaylistDetailUseCase
import com.dc.melodiasmario.core.domain.playlisttrack.usecase.LoadPlaylistUseCase
import com.dc.melodiasmario.core.domain.playlisttrack.usecase.RemovePlaylistTrackUseCase
import com.dc.melodiasmario.core.domain.realtime.usecase.ObserveGuildRealtimeUseCase
import com.dc.melodiasmario.core.domain.search.usecase.GetMusicSearchCapabilitiesUseCase
import com.dc.melodiasmario.core.domain.search.usecase.SearchMusicUseCase
import com.dc.melodiasmario.core.presentation.command.BotCommandCoordinator
import com.dc.melodiasmario.core.presentation.command.CommandLock
import com.dc.melodiasmario.core.presentation.music.search.MusicSearchController
import com.dc.melodiasmario.core.presentation.runAction
import com.dc.melodiasmario.core.presentation.runBotCommand
import kotlinx.coroutines.Job
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.SharedFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asSharedFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.map
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import org.koin.core.annotation.KoinViewModel

/**
 * Coordinates playlist track events, observable UI state, and one-time effects.
 */
@KoinViewModel
class PlaylistTrackViewModel(
    private val getPlaylistDetailUseCase: GetPlaylistDetailUseCase,
    private val removePlaylistTrackUseCase: RemovePlaylistTrackUseCase,
    private val addPlaylistTrackUseCase: AddPlaylistTrackUseCase,
    private val loadPlaylistUseCase: LoadPlaylistUseCase,
    private val getMusicSearchCapabilitiesUseCase: GetMusicSearchCapabilitiesUseCase,
    private val searchMusicUseCase: SearchMusicUseCase,
    private val observeGuildRealtimeUseCase: ObserveGuildRealtimeUseCase
) : ViewModel() {
    private val _uiState = MutableStateFlow(value = PlaylistTrackUiState())

    /**
     * Read-only presentation state observed by UI consumers.
     */
    val uiState: StateFlow<PlaylistTrackUiState> = _uiState.asStateFlow()

    private val events = MutableSharedFlow<PlaylistTrackEvent>(extraBufferCapacity = 64)
    private val _effect = MutableSharedFlow<PlaylistTrackEffect>()

    /**
     * One-time presentation outcomes observed by UI consumers.
     */
    val effect: SharedFlow<PlaylistTrackEffect> = _effect.asSharedFlow()

    private var currentGuildId: String? = null
    private var currentPlaylistId: String? = null
    private var loadJob: Job? = null
    private var commandJob: Job? = null
    private var commandVersion = 0L
    private val commandLock = CommandLock(uiState = _uiState)

    private val botCommandCoordinator = BotCommandCoordinator(
        coroutineScope = viewModelScope,
        observeGuildRealtime = observeGuildRealtimeUseCase::invoke
    )

    private val musicSearchController = MusicSearchController(
        getCapabilities = getMusicSearchCapabilitiesUseCase::invoke,
        searchMusic = searchMusicUseCase::invoke,
        uiState = _uiState,
        effects = _effect,
        failureEffect = PlaylistTrackEffect.SearchFailed,
        coroutineScope = viewModelScope,
        currentGuildIdProvider = { currentGuildId }
    )

    init {
        collectEvents()
    }

    /**
     * Submits a user or lifecycle event to the feature event-processing flow.
     */
    fun onEvent(event: PlaylistTrackEvent): Unit {
        if (event.isCommandEvent()) {
            if (!startCommandIfIdle()) {
                return
            }

            val version = commandVersion
            commandJob = viewModelScope.launch {
                try {
                    handleCommandEvent(event = event)
                } finally {
                    finishCommand(version = version)
                }
            }
            return
        }

        viewModelScope.launch {
            events.emit(value = event)
        }
    }

    private fun collectEvents() {
        viewModelScope.launch {
            events.collect { event ->
                handleEvent(event = event)
            }
        }
    }

    private suspend fun handleEvent(event: PlaylistTrackEvent) {
        if (_uiState.value.isCommandInFlight
            && event !is PlaylistTrackEvent.Load
            && event != PlaylistTrackEvent.BackClicked
        ) {
            return
        }

        when (event) {
            is PlaylistTrackEvent.Load -> loadPlaylist(guildId = event.guildId, playlistId = event.playlistId)
            PlaylistTrackEvent.BackClicked -> _effect.emit(value = PlaylistTrackEffect.NavigateBack)
            PlaylistTrackEvent.Refresh -> refreshClicked()
            PlaylistTrackEvent.MoreClicked -> moreClicked()
            PlaylistTrackEvent.DismissDialog -> closeDialog()
            PlaylistTrackEvent.AddMusicClicked -> addMusicClicked()
            is PlaylistTrackEvent.TrackMoreClicked -> trackMoreClicked(orderNumber = event.orderNumber)
            is PlaylistTrackEvent.RemoveClicked -> removeClicked(orderNumber = event.orderNumber)
            is PlaylistTrackEvent.ModeChanged -> musicSearchController.updateMode(mode = event.mode)
            is PlaylistTrackEvent.ManualDraftChanged -> updateManualDraft(value = event.value)
            is PlaylistTrackEvent.SearchQueryChanged -> musicSearchController.onQueryChanged(query = event.value)
            is PlaylistTrackEvent.ProviderChanged -> musicSearchController.onProviderChanged(providerId = event.id)
            is PlaylistTrackEvent.KindChanged -> musicSearchController.onKindChanged(kind = event.kind)
            is PlaylistTrackEvent.ResultClicked -> musicSearchController.selectResult(resultId = event.id)
            PlaylistTrackEvent.SearchSubmitted -> musicSearchController.submit()
            PlaylistTrackEvent.NextPage -> musicSearchController.loadNextPage()
            PlaylistTrackEvent.RetrySearch -> musicSearchController.retry()
            PlaylistTrackEvent.RetryCapabilities -> musicSearchController.loadCapabilities()
            // Ordering actions stay disabled until the backend supports them.
            is PlaylistTrackEvent.Move, PlaylistTrackEvent.Shuffle -> Unit
            is PlaylistTrackEvent.RemoveConfirmed,
            PlaylistTrackEvent.LoadPlaylist,
            PlaylistTrackEvent.ManualAdd,
            PlaylistTrackEvent.AddSelectedResult -> Unit // Dispatched separately by onEvent.
        }
    }

    private fun loadPlaylist(guildId: String, playlistId: String) {
        if (currentGuildId == guildId
            && currentPlaylistId == playlistId
            && _uiState.value.playlist != null
        ) {
            return
        }

        loadJob?.cancel()
        clearCommandExecution()
        musicSearchController.clear()
        currentGuildId = guildId
        currentPlaylistId = playlistId
        botCommandCoordinator.start(guildId = guildId)
        _uiState.value = PlaylistTrackUiState()
        refreshPlaylist()
    }

    private fun refreshClicked() {
        if (_uiState.value.canModify || _uiState.value.hasLoadError) {
            refreshPlaylist()
        }
    }

    private fun refreshPlaylist() {
        val playlistId = currentPlaylistId ?: return
        loadJob?.cancel()
        loadJob = viewModelScope.launch {
            fetchPlaylist(playlistId = playlistId)
        }
    }

    private suspend fun fetchPlaylist(playlistId: String) {
        getPlaylistDetailUseCase(playlistId = playlistId).map { result ->

            if (result is Resource.Success && result.data.guildId != currentGuildId) {
                Resource.Error(error = IllegalStateException("Playlist belongs to a different guild"))
            } else {
                result
            }
        }
            .runAction(
                uiState = _uiState,
                effects = _effect,
                failureEffect = PlaylistTrackEffect.OperationFailed,
                onLoading = { currentUiState ->
                    currentUiState.copy(
                        isLoading = currentUiState.playlist == null,
                        isRefreshing = currentUiState.playlist != null,
                        hasLoadError = false
                    )
                },
                onSuccessWithData = { state, playlist ->
                    state.copy(playlist = playlist, isLoading = false, isRefreshing = false, hasLoadError = false)
                },
                onError = { state, _ ->
                    state.copy(isLoading = false, isRefreshing = false, hasLoadError = true)
                }
            )
    }

    private fun moreClicked() {
        if (_uiState.value.playlist != null) {
            openDialog(dialog = PlaylistTrackDialog.More)
        }
    }

    private suspend fun addMusicClicked() {
        if (!_uiState.value.canModify) {
            return
        }

        openDialog(dialog = PlaylistTrackDialog.AddMusic)
        musicSearchController.loadCapabilities()
    }

    private fun trackMoreClicked(orderNumber: Int) {
        if (_uiState.value.canModify) {
            openDialog(dialog = PlaylistTrackDialog.TrackActions(orderNumber = orderNumber))
        }
    }

    private fun removeClicked(orderNumber: Int) {
        if (_uiState.value.canModify) {
            openDialog(dialog = PlaylistTrackDialog.ConfirmRemove(orderNumber = orderNumber))
        }
    }

    private fun updateManualDraft(value: String) {
        _uiState.update { currentUiState ->
            currentUiState.copy(
                addMusic = currentUiState.addMusic.copy(
                    manualDraft = value,
                    canSubmitManual = value.canSubmitMusicInput()
                )
            )
        }
    }

    private suspend fun handleCommandEvent(event: PlaylistTrackEvent) {
        val playlistId = currentPlaylistId ?: return
        when (event) {
            is PlaylistTrackEvent.RemoveConfirmed -> removeTrack(playlistId = playlistId, orderNumber = event.orderNumber)
            PlaylistTrackEvent.LoadPlaylist -> loadIntoQueue(playlistId = playlistId)
            PlaylistTrackEvent.ManualAdd -> addManualTrack(playlistId = playlistId)
            PlaylistTrackEvent.AddSelectedResult -> addSelectedTrack(playlistId = playlistId)
            else -> Unit
        }
    }

    private suspend fun removeTrack(playlistId: String, orderNumber: Int) {
        val track = _uiState.value.tracks.firstOrNull { track -> track.orderNumber == orderNumber } ?: return
        removePlaylistTrackUseCase(playlistId = playlistId, trackNumber = track.orderNumber).runAction(
            uiState = _uiState,
            effects = _effect,
            successEffect = PlaylistTrackEffect.TrackRemoved,
            failureEffect = PlaylistTrackEffect.OperationFailed,
            afterSuccess = { closeDialog() }
        )
        fetchPlaylist(playlistId = playlistId)
    }

    private suspend fun loadIntoQueue(playlistId: String) {
        if (_uiState.value.tracks.isEmpty()) {
            return
        }

        loadPlaylistUseCase(playlistId = playlistId).runBotCommand(
            uiState = _uiState,
            effects = _effect,
            acceptedEffect = PlaylistTrackEffect.CommandAccepted,
            failureEffect = PlaylistTrackEffect.OperationFailed,
            afterAccepted = { command ->
                botCommandCoordinator.handleCompletion(
                    command = command,
                    onSucceeded = {
                        closeDialog()
                        _effect.emit(value = PlaylistTrackEffect.PlaylistLoaded)
                    },
                    onFailed = { _effect.emit(value = PlaylistTrackEffect.OperationFailed) },
                    onTimedOut = { _effect.emit(value = PlaylistTrackEffect.CommandTimedOut) }
                )
            }
        )
        fetchPlaylist(playlistId = playlistId)
    }

    private suspend fun addManualTrack(playlistId: String) {
        val query = _uiState.value.addMusic.manualDraft.trim()

        if (!query.canSubmitMusicInput()) {
            return
        }

        addPlaylistTrackUseCase(playlistId = playlistId, query = query).runBotCommand(
            uiState = _uiState,
            effects = _effect,
            acceptedEffect = PlaylistTrackEffect.CommandAccepted,
            failureEffect = PlaylistTrackEffect.OperationFailed,
            afterAccepted = { command ->
                botCommandCoordinator.handleCompletion(
                    command = command,
                    onSucceeded = {
                        closeDialog()
                        _effect.emit(value = PlaylistTrackEffect.TrackAdded)
                    },
                    onFailed = { _effect.emit(value = PlaylistTrackEffect.OperationFailed) },
                    onTimedOut = { _effect.emit(value = PlaylistTrackEffect.CommandTimedOut) }
                )
            }
        )
        fetchPlaylist(playlistId = playlistId)
    }

    private suspend fun addSelectedTrack(playlistId: String) {
        val music = _uiState.value.addMusic

        val result = music.results
            .firstOrNull { searchResult -> searchResult.id == music.selectedResultId }
            ?: return

        val url = result.canonicalUrl
            ?.takeIf { textValue -> result.canEnqueue && textValue.isNotBlank() }
            ?: return

        addPlaylistTrackUseCase(playlistId = playlistId, query = url).runBotCommand(
            uiState = _uiState,
            effects = _effect,
            acceptedEffect = PlaylistTrackEffect.CommandAccepted,
            failureEffect = PlaylistTrackEffect.OperationFailed,
            afterAccepted = { command ->
                botCommandCoordinator.handleCompletion(
                    command = command,
                    onSucceeded = {
                        closeDialog()
                        _effect.emit(value = PlaylistTrackEffect.TrackAdded)
                    },
                    onFailed = { _effect.emit(value = PlaylistTrackEffect.OperationFailed) },
                    onTimedOut = { _effect.emit(value = PlaylistTrackEffect.CommandTimedOut) }
                )
            }
        )
        fetchPlaylist(playlistId = playlistId)
    }

    private fun openDialog(dialog: PlaylistTrackDialog) {
        _uiState.update { currentUiState -> currentUiState.copy(dialog = dialog) }
    }

    private fun closeDialog() {
        musicSearchController.clear()
        _uiState.update { currentUiState -> currentUiState.copy(dialog = PlaylistTrackDialog.None) }
    }

    private fun startCommandIfIdle(): Boolean {
        if (currentPlaylistId == null
            || !_uiState.value.canModify
            || !commandLock.startIfIdle()) {
            return false
        }

        loadJob?.cancel()
        musicSearchController.cancel()
        commandVersion++
        return true
    }

    private fun finishCommand(version: Long) {
        if (version == commandVersion) {
            commandLock.finish()
        }
    }

    private fun clearCommandExecution() {
        commandVersion++
        commandJob?.cancel()
        commandJob = null
        commandLock.finish()
    }

    override fun onCleared(): Unit {
        botCommandCoordinator.clear()
        loadJob?.cancel()
        clearCommandExecution()
        musicSearchController.cancel()
        super.onCleared()
    }
}
