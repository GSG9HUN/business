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
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asSharedFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.map
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import org.koin.core.annotation.KoinViewModel

@KoinViewModel
class PlaylistTrackViewModel(
    private val getPlaylistDetailUseCase: GetPlaylistDetailUseCase,
    private val removePlaylistTrackUseCase: RemovePlaylistTrackUseCase,
    private val addPlaylistTrackUseCase: AddPlaylistTrackUseCase,
    private val loadPlaylistUseCase: LoadPlaylistUseCase,
    private val getMusicSearchCapabilitiesUseCase: GetMusicSearchCapabilitiesUseCase,
    private val searchMusicUseCase: SearchMusicUseCase,
    private val observeGuildRealtimeUseCase: ObserveGuildRealtimeUseCase,
) : ViewModel() {
    private val _uiState = MutableStateFlow(PlaylistTrackUiState())
    val uiState: StateFlow<PlaylistTrackUiState> = _uiState.asStateFlow()

    private val events = MutableSharedFlow<PlaylistTrackEvent>(extraBufferCapacity = 64)
    private val _effect = MutableSharedFlow<PlaylistTrackEffect>()
    val effect = _effect.asSharedFlow()

    private var currentGuildId: String? = null
    private var currentPlaylistId: String? = null
    private var loadJob: Job? = null
    private var commandJob: Job? = null
    private var commandVersion = 0L
    private val commandLock = CommandLock(_uiState)

    private val commandCoordinator = BotCommandCoordinator(
        scope = viewModelScope,
        observeGuildRealtime = observeGuildRealtimeUseCase::invoke,
    )

    private val searchController = MusicSearchController(
        getCapabilities = getMusicSearchCapabilitiesUseCase::invoke,
        searchMusic = searchMusicUseCase::invoke,
        state = _uiState,
        effects = _effect,
        failureEffect = PlaylistTrackEffect.SearchFailed,
        scope = viewModelScope,
        currentGuildIdProvider = { currentGuildId },
    )

    init {
        collectEvents()
    }

    fun onEvent(event: PlaylistTrackEvent) {
        if (event.isCommandEvent()) {
            if (!startCommandIfIdle()) {
                return
            }

            val version = commandVersion
            commandJob = viewModelScope.launch {
                try {
                    handleCommandEvent(event)
                } finally {
                    finishCommand(version)
                }
            }
            return
        }

        viewModelScope.launch {
            events.emit(event)
        }
    }

    private fun collectEvents() {
        viewModelScope.launch {
            events.collect { event ->
                handleEvent(event)
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
            is PlaylistTrackEvent.Load -> loadPlaylist(event.guildId, event.playlistId)
            PlaylistTrackEvent.BackClicked -> _effect.emit(PlaylistTrackEffect.NavigateBack)
            PlaylistTrackEvent.Refresh -> refreshClicked()
            PlaylistTrackEvent.MoreClicked -> moreClicked()
            PlaylistTrackEvent.DismissDialog -> closeDialog()
            PlaylistTrackEvent.AddMusicClicked -> addMusicClicked()
            is PlaylistTrackEvent.TrackMoreClicked -> trackMoreClicked(event.orderNumber)
            is PlaylistTrackEvent.RemoveClicked -> removeClicked(event.orderNumber)
            is PlaylistTrackEvent.ModeChanged -> searchController.updateMode(event.mode)
            is PlaylistTrackEvent.ManualDraftChanged -> updateManualDraft(event.value)
            is PlaylistTrackEvent.SearchQueryChanged -> searchController.onQueryChanged(event.value)
            is PlaylistTrackEvent.ProviderChanged -> searchController.onProviderChanged(event.id)
            is PlaylistTrackEvent.KindChanged -> searchController.onKindChanged(event.kind)
            is PlaylistTrackEvent.ResultClicked -> searchController.selectResult(event.id)
            PlaylistTrackEvent.SearchSubmitted -> searchController.submit()
            PlaylistTrackEvent.NextPage -> searchController.loadNextPage()
            PlaylistTrackEvent.RetrySearch -> searchController.retry()
            PlaylistTrackEvent.RetryCapabilities -> searchController.loadCapabilities()
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
        searchController.clear()
        currentGuildId = guildId
        currentPlaylistId = playlistId
        commandCoordinator.start(guildId)
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
            fetchPlaylist(playlistId)
        }
    }

    private suspend fun fetchPlaylist(playlistId: String) {
        getPlaylistDetailUseCase(playlistId).map { result ->
            if (result is Resource.Success && result.data.guildId != currentGuildId) {
                Resource.Error(IllegalStateException("Playlist belongs to a different guild"))
            } else {
                result
            }
        }.runAction(
            state = _uiState,
            effects = _effect,
            failureEffect = PlaylistTrackEffect.OperationFailed,
            onLoading = {
                it.copy(
                    isLoading = it.playlist == null,
                    isRefreshing = it.playlist != null,
                    hasLoadError = false,
                )
            },
            onSuccessWithData = { state, playlist ->
                state.copy(playlist = playlist, isLoading = false, isRefreshing = false, hasLoadError = false)
            },
            onError = { state, _ ->
                state.copy(isLoading = false, isRefreshing = false, hasLoadError = true)
            },
        )
    }

    private fun moreClicked() {
        if (_uiState.value.playlist != null) {
            openDialog(PlaylistTrackDialog.More)
        }
    }

    private suspend fun addMusicClicked() {
        if (!_uiState.value.canModify) {
            return
        }

        openDialog(PlaylistTrackDialog.AddMusic)
        searchController.loadCapabilities()
    }

    private fun trackMoreClicked(orderNumber: Int) {
        if (_uiState.value.canModify) {
            openDialog(PlaylistTrackDialog.TrackActions(orderNumber))
        }
    }

    private fun removeClicked(orderNumber: Int) {
        if (_uiState.value.canModify) {
            openDialog(PlaylistTrackDialog.ConfirmRemove(orderNumber))
        }
    }

    private fun updateManualDraft(value: String) {
        _uiState.update {
            it.copy(addMusic = it.addMusic.copy(
                manualDraft = value,
                canSubmitManual = value.canSubmitMusicInput(),
            ))
        }
    }

    private suspend fun handleCommandEvent(event: PlaylistTrackEvent) {
        val playlistId = currentPlaylistId ?: return
        when (event) {
            is PlaylistTrackEvent.RemoveConfirmed -> removeTrack(playlistId, event.orderNumber)
            PlaylistTrackEvent.LoadPlaylist -> loadIntoQueue(playlistId)
            PlaylistTrackEvent.ManualAdd -> addManualTrack(playlistId)
            PlaylistTrackEvent.AddSelectedResult -> addSelectedTrack(playlistId)
            else -> Unit
        }
    }

    private suspend fun removeTrack(playlistId: String, orderNumber: Int) {
        val track = _uiState.value.tracks.firstOrNull { it.orderNumber == orderNumber } ?: return
        removePlaylistTrackUseCase(playlistId, track.orderNumber).runAction(
            state = _uiState,
            effects = _effect,
            successEffect = PlaylistTrackEffect.TrackRemoved,
            failureEffect = PlaylistTrackEffect.OperationFailed,
            afterSuccess = { closeDialog() },
        )
        fetchPlaylist(playlistId)
    }

    private suspend fun loadIntoQueue(playlistId: String) {
        if (_uiState.value.tracks.isEmpty()) {
            return
        }

        loadPlaylistUseCase(playlistId).runBotCommand(
            state = _uiState,
            effects = _effect,
            acceptedEffect = PlaylistTrackEffect.CommandAccepted,
            failureEffect = PlaylistTrackEffect.OperationFailed,
            afterAccepted = { command ->
                commandCoordinator.handleCompletion(
                    command = command,
                    onSucceeded = {
                        closeDialog()
                        _effect.emit(PlaylistTrackEffect.PlaylistLoaded)
                    },
                    onFailed = { _effect.emit(PlaylistTrackEffect.OperationFailed) },
                    onTimedOut = { _effect.emit(PlaylistTrackEffect.CommandTimedOut) },
                )
            },
        )
        fetchPlaylist(playlistId)
    }

    private suspend fun addManualTrack(playlistId: String) {
        val query = _uiState.value.addMusic.manualDraft.trim()
        if (!query.canSubmitMusicInput()) {
            return
        }

        addPlaylistTrackUseCase(playlistId, query).runBotCommand(
            state = _uiState,
            effects = _effect,
            acceptedEffect = PlaylistTrackEffect.CommandAccepted,
            failureEffect = PlaylistTrackEffect.OperationFailed,
            afterAccepted = { command ->
                commandCoordinator.handleCompletion(
                    command = command,
                    onSucceeded = {
                        closeDialog()
                        _effect.emit(PlaylistTrackEffect.TrackAdded)
                    },
                    onFailed = { _effect.emit(PlaylistTrackEffect.OperationFailed) },
                    onTimedOut = { _effect.emit(PlaylistTrackEffect.CommandTimedOut) },
                )
            },
        )
        fetchPlaylist(playlistId)
    }

    private suspend fun addSelectedTrack(playlistId: String) {
        val music = _uiState.value.addMusic

        val result = music.results
            .firstOrNull { it.id == music.selectedResultId }
            ?: return

        val url = result.canonicalUrl
            ?.takeIf { result.canEnqueue && it.isNotBlank() }
            ?: return

        addPlaylistTrackUseCase(playlistId, url).runBotCommand(
            state = _uiState,
            effects = _effect,
            acceptedEffect = PlaylistTrackEffect.CommandAccepted,
            failureEffect = PlaylistTrackEffect.OperationFailed,
            afterAccepted = { command ->
                commandCoordinator.handleCompletion(
                    command = command,
                    onSucceeded = {
                        closeDialog()
                        _effect.emit(PlaylistTrackEffect.TrackAdded)
                    },
                    onFailed = { _effect.emit(PlaylistTrackEffect.OperationFailed) },
                    onTimedOut = { _effect.emit(PlaylistTrackEffect.CommandTimedOut) },
                )
            },
        )
        fetchPlaylist(playlistId)
    }

    private fun openDialog(dialog: PlaylistTrackDialog) {
        _uiState.update { it.copy(dialog = dialog) }
    }

    private fun closeDialog() {
        searchController.clear()
        _uiState.update { it.copy(dialog = PlaylistTrackDialog.None) }
    }

    private fun startCommandIfIdle(): Boolean {
        if (currentPlaylistId == null || !_uiState.value.canModify || !commandLock.startIfIdle()) {
            return false
        }

        loadJob?.cancel()
        searchController.cancel()
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

    override fun onCleared() {
        commandCoordinator.clear()
        loadJob?.cancel()
        clearCommandExecution()
        searchController.cancel()
        super.onCleared()
    }
}
