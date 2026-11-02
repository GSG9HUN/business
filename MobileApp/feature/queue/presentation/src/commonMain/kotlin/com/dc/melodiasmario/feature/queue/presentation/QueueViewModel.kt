package com.dc.melodiasmario.feature.queue.presentation

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.commonui.music.validation.canSubmitMusicInput
import com.dc.melodiasmario.core.domain.currentuser.usecase.GetCurrentUserUseCase
import com.dc.melodiasmario.core.domain.playbackmonitor.usecase.ObserveCurrentPlaybackMonitorUseCase
import com.dc.melodiasmario.core.domain.playbackmonitor.usecase.RefreshCurrentPlaybackMonitorUseCase
import com.dc.melodiasmario.core.domain.playbackmonitor.usecase.StartCurrentPlaybackMonitorUseCase
import com.dc.melodiasmario.core.domain.queue.usecase.AddToQueueUseCase
import com.dc.melodiasmario.core.domain.queue.usecase.ClearQueueUseCase
import com.dc.melodiasmario.core.domain.queue.usecase.MoveTrackToIndexUseCase
import com.dc.melodiasmario.core.domain.queue.usecase.RemoveFromQueueUseCase
import com.dc.melodiasmario.core.domain.queue.usecase.ShuffleQueueUseCase
import com.dc.melodiasmario.core.domain.realtime.usecase.ObserveGuildRealtimeUseCase
import com.dc.melodiasmario.core.domain.search.usecase.GetMusicSearchCapabilitiesUseCase
import com.dc.melodiasmario.core.domain.search.usecase.SearchMusicUseCase
import com.dc.melodiasmario.core.model.playbackmonitor.CurrentPlaybackMonitorState
import com.dc.melodiasmario.core.presentation.command.BotCommandCoordinator
import com.dc.melodiasmario.core.presentation.command.CommandLock
import com.dc.melodiasmario.core.presentation.music.search.MusicSearchController
import com.dc.melodiasmario.core.presentation.playbackmonitor.PlaybackMonitorObserver
import com.dc.melodiasmario.feature.queue.presentation.command.QueueCommandHandler
import com.dc.melodiasmario.feature.queue.presentation.playlist.QueuePlaylistController
import com.dc.melodiasmario.feature.queue.presentation.playlist.QueuePlaylistSaveManager
import kotlinx.coroutines.Job
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.SharedFlow
import kotlinx.coroutines.flow.SharingStarted.Companion.WhileSubscribed
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asSharedFlow
import kotlinx.coroutines.flow.onStart
import kotlinx.coroutines.flow.stateIn
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import org.koin.core.annotation.KoinViewModel

/**
 * Coordinates queue events, observable UI state, and one-time effects.
 */
@KoinViewModel
class QueueViewModel(
    private val getCurrentUserUseCase: GetCurrentUserUseCase,
    private val observeCurrentPlaybackMonitorUseCase: ObserveCurrentPlaybackMonitorUseCase,
    private val startCurrentPlaybackMonitorUseCase: StartCurrentPlaybackMonitorUseCase,
    private val refreshCurrentPlaybackMonitorUseCase: RefreshCurrentPlaybackMonitorUseCase,
    private val observeGuildRealtimeUseCase: ObserveGuildRealtimeUseCase,
    private val addToQueueUseCase: AddToQueueUseCase,
    private val removeFromQueueUseCase: RemoveFromQueueUseCase,
    private val clearQueueUseCase: ClearQueueUseCase,
    private val shuffleQueueUseCase: ShuffleQueueUseCase,
    private val moveTrackToIndexUseCase: MoveTrackToIndexUseCase,
    private val queuePlaylistSaveManager: QueuePlaylistSaveManager,
    private val getMusicSearchCapabilitiesUseCase: GetMusicSearchCapabilitiesUseCase,
    private val searchMusicUseCase: SearchMusicUseCase
) : ViewModel() {
    private val _uiState = MutableStateFlow(value = QueueUiState())

    /**
     * Read-only presentation state observed by UI consumers.
     */
    val uiState: StateFlow<QueueUiState> = _uiState
        .onStart {
            playbackMonitorObserver.start()
        }
        .stateIn(
            scope = viewModelScope,
            started = WhileSubscribed(stopTimeoutMillis = 5_000),
            initialValue = QueueUiState()
        )
    private val events = MutableSharedFlow<QueueEvent>(extraBufferCapacity = 64)
    private val playbackMonitorObserver = PlaybackMonitorObserver(
        coroutineScope = viewModelScope,
        observePlayback = observeCurrentPlaybackMonitorUseCase::invoke,
        onPlayback = ::updatePlayback
    )

    private val _effect = MutableSharedFlow<QueueEffect>()

    /**
     * One-time presentation outcomes observed by UI consumers.
     */
    val effect: SharedFlow<QueueEffect> = _effect.asSharedFlow()

    private var currentGuildId: String? = null
    private var currentUserId: String? = null
    private var commandVersion = 0L
    private var commandJob: Job? = null
    private val commandLock = CommandLock(uiState = _uiState)

    private val botCommandCoordinator = BotCommandCoordinator(
        coroutineScope = viewModelScope,
        observeGuildRealtime = observeGuildRealtimeUseCase::invoke,
        currentUserIdProvider = { currentUserId }
    )

    private val queueCommandHandler = QueueCommandHandler(
        addToQueueUseCase = addToQueueUseCase,
        removeFromQueueUseCase = removeFromQueueUseCase,
        clearQueueUseCase = clearQueueUseCase,
        shuffleQueueUseCase = shuffleQueueUseCase,
        moveTrackToIndexUseCase = moveTrackToIndexUseCase,
        uiState = _uiState,
        effects = _effect,
        currentGuildIdProvider = { currentGuildId },
        onTrackPendingCommand = { command ->
            botCommandCoordinator.handleCompletion(
                command = command,
                onSucceeded = { _effect.emit(value = QueueEffect.BotCommandSucceeded(commandType = command.type)) },
                onFailed = { errorKey ->
                    _effect.emit(
                        value = QueueEffect.BotCommandFailed(
                            commandType = command.type,
                            errorKey = errorKey
                        )
                    )
                },
                onTimedOut = { _effect.emit(value = QueueEffect.BotCommandTimedOut) },
                onCompleted = { refreshCurrentPlaybackMonitorUseCase() }
            )
        },
        onCloseDialog = ::closeDialog,
        onMoveFailed = { refreshCurrentPlaybackMonitorUseCase() }
    )

    private val musicSearchController = MusicSearchController(
        getCapabilities = getMusicSearchCapabilitiesUseCase::invoke,
        searchMusic = searchMusicUseCase::invoke,
        uiState = _uiState,
        effects = _effect,
        failureEffect = QueueEffect.SearchFailed,
        coroutineScope = viewModelScope,
        currentGuildIdProvider = { currentGuildId }
    )

    private val queuePlaylistController = QueuePlaylistController(
        queuePlaylistSaveManager = queuePlaylistSaveManager,
        uiState = _uiState,
        effects = _effect,
        currentGuildIdProvider = { currentGuildId },
        onCloseDialog = ::closeDialog
    )

    init {
        collectEvents()
    }

    /**
     * Submits a user or lifecycle event to the feature event-processing flow.
     */
    fun onEvent(event: QueueEvent): Unit {
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

    private suspend fun handleEvent(event: QueueEvent) {
        when (event) {
            is QueueEvent.LoadQueue -> loadQueue(guildId = event.guildId)

            QueueEvent.RefreshClicked -> refreshClicked()

            QueueEvent.GuildClicked -> {
                _effect.emit(value = QueueEffect.NavigateToGuildSelector)
            }

            QueueEvent.AddMusicClicked -> addMusicClicked()

            QueueEvent.MoreClicked -> {
                openDialog(dialog = QueueDialog.MoreActions)
            }

            QueueEvent.DialogDismissed -> {
                if (!_uiState.value.isCommandInFlight) {
                    closeDialog()
                }
            }

            is QueueEvent.ManualDraftChanged -> {
                updateManualDraft(value = event.value)
            }

            is QueueEvent.AddMusicModeChanged -> musicSearchController.updateMode(mode = event.mode)

            is QueueEvent.SearchQueryChanged -> {
                musicSearchController.onQueryChanged(query = event.query)
            }

            QueueEvent.SearchSubmitted -> musicSearchController.submit()

            QueueEvent.SearchRetryClicked -> musicSearchController.retry()

            QueueEvent.SearchNextPageRequested -> musicSearchController.loadNextPage()

            QueueEvent.SearchCapabilitiesRetryClicked -> musicSearchController.loadCapabilities()

            is QueueEvent.SearchProviderChanged -> musicSearchController.onProviderChanged(providerId = event.providerId)

            is QueueEvent.SearchKindChanged -> musicSearchController.onKindChanged(kind = event.kind)

            is QueueEvent.SearchResultClicked -> musicSearchController.selectResult(resultId = event.resultId)

            QueueEvent.MakePlaylistFromQueueClicked -> makePlaylistClicked()

            is QueueEvent.PlaylistNameChanged -> {
                _uiState.update { currentUiState ->
                    currentUiState.copy(
                        playlistNameDraft = event.value
                    )
                }
            }

            QueueEvent.ManualAddClicked,
            QueueEvent.SelectedSearchResultAddClicked,
            QueueEvent.ClearQueueClicked,
            QueueEvent.ShuffleQueueClicked,
            QueueEvent.CreatePlaylistConfirmed,
            is QueueEvent.RemoveTrackClicked,
            is QueueEvent.MoveTrackClicked -> {
                runCommandIfIdle { handleCommandEvent(event = event) }
            }
        }
    }

    private suspend fun loadQueue(guildId: String) {
        if (currentGuildId != guildId) {
            clearCommandExecution()
            musicSearchController.resetForGuildChange(
                newGuildId = guildId,
                currentGuildId = currentGuildId
            )

            currentGuildId = guildId
            currentUserId = null

            _uiState.value = QueueUiState(guildId = guildId)
        }

        botCommandCoordinator.start(guildId = guildId)
        startCurrentPlaybackMonitorUseCase(guildId = guildId)

        updatePlayback(playback = observeCurrentPlaybackMonitorUseCase().value)

        loadCurrentUser()
    }

    private fun updatePlayback(playback: CurrentPlaybackMonitorState) {
        val guildId = currentGuildId ?: return

        if (playback.guildId != guildId) {
            return
        }

        val track = playback.currentTrack?.takeIf { currentTrack -> currentTrack.guildId == guildId }

        _uiState.update { currentUiState ->
            currentUiState.copy(
                isLoading = track == null && playback.isRefreshing,
                errorMessage = playback.errorMessage,
                guildName = track?.guildName.orEmpty(),
                guildIconUrl = track?.guildIconUrl,
                tracks = track?.queuedTracks.orEmpty()
            )
        }
    }

    private suspend fun loadCurrentUser() {
        getCurrentUserUseCase().collect { result ->

            if (result is Resource.Success) {
                currentUserId = result.data.id
            }
        }
    }

    private suspend fun addMusicClicked() {
        if (_uiState.value.isCommandInFlight) {
            return
        }

        openDialog(dialog = QueueDialog.AddMusic)
        musicSearchController.loadCapabilities()
    }

    private fun refreshClicked() {
        if (_uiState.value.isCommandInFlight) {
            return
        }

        closeDialog()

        _uiState.update { currentUiState -> currentUiState.copy(isLoading = currentUiState.tracks.isEmpty(), errorMessage = null) }

        refreshCurrentPlaybackMonitorUseCase()
    }

    private fun makePlaylistClicked() {
        if (_uiState.value.isCommandInFlight || _uiState.value.tracks.isEmpty()) {
            return
        }

        _uiState.update { currentUiState ->
            currentUiState.copy(
                dialog = QueueDialog.CreatePlaylist,
                playlistNameDraft = ""
            )
        }
    }

    private suspend fun handleCommandEvent(event: QueueEvent) {
        when (event) {
            QueueEvent.CreatePlaylistConfirmed -> {
                queuePlaylistController.createPlaylist()
            }

            else -> queueCommandHandler.handle(event)
        }
    }

    private suspend fun runCommandIfIdle(action: suspend () -> Unit) {
        if (!startCommandIfIdle()) {
            return
        }

        val version = commandVersion
        try {
            action()
        } finally {
            finishCommand(version = version)
        }
    }

    private fun startCommandIfIdle(): Boolean {
        if (currentGuildId == null || _uiState.value.isLoading) {
            return false
        }

        if (!commandLock.startIfIdle()) {
            return false
        }

        commandVersion += 1
        return true
    }

    private fun finishCommand(version: Long) {
        if (version == commandVersion) {
            commandLock.finish()
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

    private fun openDialog(dialog: QueueDialog) {
        _uiState.update { currentUiState -> currentUiState.copy(dialog = dialog) }
    }

    private fun closeDialog() {
        musicSearchController.clear()

        _uiState.update { currentUiState ->
            currentUiState.copy(
                dialog = QueueDialog.None,
                playlistNameDraft = ""
            )
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
        playbackMonitorObserver.stop()

        clearCommandExecution()
        musicSearchController.cancel()

        super.onCleared()
    }

}
