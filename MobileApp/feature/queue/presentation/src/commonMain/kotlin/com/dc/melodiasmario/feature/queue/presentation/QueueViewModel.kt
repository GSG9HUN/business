package com.dc.melodiasmario.feature.queue.presentation

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.presentation.command.CommandLock
import com.dc.melodiasmario.core.presentation.music.search.MusicSearchController
import com.dc.melodiasmario.core.commonui.music.validation.canSubmitMusicInput
import com.dc.melodiasmario.core.presentation.BotCommandRealtimeObserver
import com.dc.melodiasmario.core.domain.currentuser.usecase.GetCurrentUserUseCase
import com.dc.melodiasmario.core.domain.playbackmonitor.usecase.ObserveCurrentPlaybackMonitorUseCase
import com.dc.melodiasmario.core.domain.playbackmonitor.usecase.RefreshCurrentPlaybackMonitorUseCase
import com.dc.melodiasmario.core.domain.playbackmonitor.usecase.StartCurrentPlaybackMonitorUseCase
import com.dc.melodiasmario.core.domain.playlist.usecase.CreatePlaylistFromQueueUseCase
import com.dc.melodiasmario.core.domain.queue.usecase.AddToQueueUseCase
import com.dc.melodiasmario.core.domain.queue.usecase.ClearQueueUseCase
import com.dc.melodiasmario.core.domain.queue.usecase.MoveTrackToIndexUseCase
import com.dc.melodiasmario.core.domain.queue.usecase.RemoveFromQueueUseCase
import com.dc.melodiasmario.core.domain.queue.usecase.ShuffleQueueUseCase
import com.dc.melodiasmario.core.domain.realtime.usecase.ObserveGuildRealtimeUseCase
import com.dc.melodiasmario.core.domain.search.usecase.GetMusicSearchCapabilitiesUseCase
import com.dc.melodiasmario.core.domain.search.usecase.SearchMusicUseCase
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.model.playbackmonitor.CurrentPlaybackMonitorState
import com.dc.melodiasmario.core.model.realtime.BotControlCommandRealtimeEvent
import com.dc.melodiasmario.feature.queue.presentation.command.QueueCommandHandler
import com.dc.melodiasmario.core.presentation.command.PendingBotCommandTracker
import com.dc.melodiasmario.feature.queue.presentation.playlist.QueuePlaylistController
import kotlinx.coroutines.Job
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.SharingStarted.Companion.WhileSubscribed
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asSharedFlow
import kotlinx.coroutines.flow.onStart
import kotlinx.coroutines.flow.stateIn
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import org.koin.core.annotation.KoinViewModel

@KoinViewModel
class QueueViewModel(
    private val getCurrentUserUseCase: GetCurrentUserUseCase,
    private val observeCurrentPlaybackMonitorUseCase: ObserveCurrentPlaybackMonitorUseCase,
    private val startCurrentPlaybackMonitorUseCase: StartCurrentPlaybackMonitorUseCase,
    private val refreshCurrentPlaybackMonitorUseCase: RefreshCurrentPlaybackMonitorUseCase,
    observeGuildRealtimeUseCase: ObserveGuildRealtimeUseCase,
    addToQueueUseCase: AddToQueueUseCase,
    removeFromQueueUseCase: RemoveFromQueueUseCase,
    clearQueueUseCase: ClearQueueUseCase,
    shuffleQueueUseCase: ShuffleQueueUseCase,
    moveTrackToIndexUseCase: MoveTrackToIndexUseCase,
    createPlaylistFromQueueUseCase: CreatePlaylistFromQueueUseCase,
    getMusicSearchCapabilitiesUseCase: GetMusicSearchCapabilitiesUseCase,
    searchMusicUseCase: SearchMusicUseCase,
) : ViewModel() {
    private val _uiState = MutableStateFlow(QueueUiState())

    val uiState: StateFlow<QueueUiState> = _uiState
        .onStart {
            collectPlaybackMonitor()
        }.stateIn(
            scope = viewModelScope,
            started = WhileSubscribed(5_000),
            initialValue = QueueUiState(),
        )
    private val events =
        MutableSharedFlow<QueueEvent>(extraBufferCapacity = 64)

    private val _effect = MutableSharedFlow<QueueEffect>()
    val effect = _effect.asSharedFlow()

    private var currentGuildId: String? = null
    private var currentUserId: String? = null

    private var playbackMonitorJob: Job? = null
    private var commandVersion = 0L
    private var commandJob: Job? = null
    private val commandLock = CommandLock(_uiState)

    private val pendingCommandTracker = PendingBotCommandTracker(
        scope = viewModelScope,
        timeoutMillis = COMMAND_TERMINAL_EVENT_TIMEOUT,
        onTimeout = {
            refreshCurrentPlaybackMonitorUseCase()
            _effect.emit(QueueEffect.BotCommandTimedOut)
        },
        onFinished = commandLock::finish,
    )

    private val commandRealtimeObserver = BotCommandRealtimeObserver(
        scope = viewModelScope,
        observeGuildRealtime = observeGuildRealtimeUseCase::invoke,
        onSucceeded = ::handleBotControlCommandSucceeded,
        onFailed = ::handleBotControlCommandFailed,
    )

    private val commandHandler = QueueCommandHandler(
        addToQueueUseCase = addToQueueUseCase,
        removeFromQueueUseCase = removeFromQueueUseCase,
        clearQueueUseCase = clearQueueUseCase,
        shuffleQueueUseCase = shuffleQueueUseCase,
        moveTrackToIndexUseCase = moveTrackToIndexUseCase,
        state = _uiState,
        effects = _effect,
        currentGuildIdProvider = { currentGuildId },
        onTrackPendingCommand = ::trackPendingCommand,
        onCloseDialog = ::closeDialog,
        onMoveFailed = { refreshCurrentPlaybackMonitorUseCase() },
    )

    private val searchController = MusicSearchController(
        getCapabilities = getMusicSearchCapabilitiesUseCase::invoke,
        searchMusic = searchMusicUseCase::invoke,
        state = _uiState,
        effects = _effect,
        failureEffect = QueueEffect.SearchFailed,
        scope = viewModelScope,
        currentGuildIdProvider = { currentGuildId },
    )

    private val playlistController = QueuePlaylistController(
        createPlaylistFromQueueUseCase = createPlaylistFromQueueUseCase,
        state = _uiState,
        effects = _effect,
        currentGuildIdProvider = { currentGuildId },
        onCloseDialog = ::closeDialog,
    )

    init {
        collectEvents()
    }


    fun onEvent(event: QueueEvent) {
        if (event.isCommandEvent()) {
            if (!startCommandIfIdle()) {
                return
            }

            val version = commandVersion
            commandJob = viewModelScope.launch {
                try {
                    handleCommandEvent(event)
                } finally {
                    finishCommandIfNoPendingCommand(version)
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

    private suspend fun handleEvent(event: QueueEvent) {
        when (event) {
            is QueueEvent.LoadQueue -> loadQueue(event.guildId)

            QueueEvent.RefreshClicked -> refreshClicked()

            QueueEvent.GuildClicked -> {
                _effect.emit(QueueEffect.NavigateToGuildSelector)
            }

            QueueEvent.AddMusicClicked -> addMusicClicked()

            QueueEvent.MoreClicked -> {
                openDialog(QueueDialog.MoreActions)
            }

            QueueEvent.DialogDismissed -> {
                if (!_uiState.value.isCommandInFlight) {
                    closeDialog()
                }
            }

            is QueueEvent.ManualDraftChanged -> {
                updateManualDraft(event.value)
            }

            is QueueEvent.AddMusicModeChanged -> searchController.updateMode(event.mode)

            is QueueEvent.SearchQueryChanged -> {
                searchController.onQueryChanged(event.query)
            }

            QueueEvent.SearchSubmitted -> searchController.submit()

            QueueEvent.SearchRetryClicked -> searchController.retry()

            QueueEvent.SearchNextPageRequested -> searchController.loadNextPage()

            QueueEvent.SearchCapabilitiesRetryClicked -> searchController.loadCapabilities()

            is QueueEvent.SearchProviderChanged -> searchController.onProviderChanged(event.providerId)

            is QueueEvent.SearchKindChanged -> searchController.onKindChanged(event.kind)

            is QueueEvent.SearchResultClicked -> searchController.selectResult(event.resultId)

            QueueEvent.MakePlaylistFromQueueClicked -> makePlaylistClicked()

            is QueueEvent.PlaylistNameChanged -> {
                _uiState.update {
                    it.copy(
                        playlistNameDraft = event.value,
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
                runCommandIfIdle { handleCommandEvent(event) }
            }
        }
    }

    private suspend fun loadQueue(guildId: String) {
        if (currentGuildId != guildId) {
            clearCommandExecution()
            searchController.resetForGuildChange(
                newGuildId = guildId,
                currentGuildId = currentGuildId,
            )

            currentGuildId = guildId
            currentUserId = null

            _uiState.value = QueueUiState(guildId = guildId)
        }

        commandRealtimeObserver.start(guildId)
        startCurrentPlaybackMonitorUseCase(guildId)

        updatePlayback(observeCurrentPlaybackMonitorUseCase().value)

        loadCurrentUser()
    }

    private fun updatePlayback(playback: CurrentPlaybackMonitorState) {
        val guildId = currentGuildId ?: return

        if (playback.guildId != guildId) {
            return
        }

        val track = playback.currentTrack?.takeIf { it.guildId == guildId }

        _uiState.update {
            it.copy(
                isLoading = track == null && playback.isRefreshing,
                errorMessage = playback.errorMessage,
                guildName = track?.guildName.orEmpty(),
                guildIconUrl = track?.guildIconUrl,
                tracks = track?.queuedTracks.orEmpty(),
            )
        }
    }

    private fun collectPlaybackMonitor() {
        if (playbackMonitorJob?.isActive == true) {
            return
        }

        playbackMonitorJob = viewModelScope.launch {
            observeCurrentPlaybackMonitorUseCase().collect {
                updatePlayback(it)
            }
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

        openDialog(QueueDialog.AddMusic)
        searchController.loadCapabilities()
    }

    private fun refreshClicked() {
        if (_uiState.value.isCommandInFlight) {
            return
        }

        closeDialog()

        _uiState.update { it.copy(isLoading = it.tracks.isEmpty(), errorMessage = null) }

        refreshCurrentPlaybackMonitorUseCase()
    }

    private fun makePlaylistClicked() {
        if (_uiState.value.isCommandInFlight || _uiState.value.tracks.isEmpty()) {
            return
        }

        _uiState.update {
            it.copy(
                dialog = QueueDialog.CreatePlaylist,
                playlistNameDraft = ""
            )
        }
    }

    private suspend fun handleCommandEvent(event: QueueEvent) {
        when (event) {
            QueueEvent.CreatePlaylistConfirmed -> {
                playlistController.createPlaylist()
            }

            else -> commandHandler.handle(event)
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
            finishCommandIfNoPendingCommand(version)
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

    private fun finishCommandIfNoPendingCommand(version: Long) {
        if (version == commandVersion) {
            pendingCommandTracker.finishIfNoPendingCommand()
        }
    }

    private suspend fun trackPendingCommand(command: BotControlCommand) {
        when (command.state.lowercase()) {
            "done" -> {
                pendingCommandTracker.finish()
                refreshCurrentPlaybackMonitorUseCase()
                _effect.emit(QueueEffect.BotCommandSucceeded(command.type))
            }

            "failed" -> {
                pendingCommandTracker.finish()
                refreshCurrentPlaybackMonitorUseCase()
                _effect.emit(QueueEffect.BotCommandFailed(command.type, null))
            }

            else -> pendingCommandTracker.track(command.commandId)
        }
    }

    private fun matchesPendingCommand(event: BotControlCommandRealtimeEvent): Boolean {
        return event.guildId == currentGuildId && pendingCommandTracker.matches(
            eventCommandId = event.commandId,
            eventUserId = event.userId,
            currentUserId = currentUserId,
        )
    }

    private suspend fun handleBotControlCommandSucceeded(commandEvent: BotControlCommandRealtimeEvent) {
        if (!matchesPendingCommand(commandEvent)) {
            return
        }
        pendingCommandTracker.finish()
        refreshCurrentPlaybackMonitorUseCase()
        _effect.emit(QueueEffect.BotCommandSucceeded(commandEvent.type))
    }

    private suspend fun handleBotControlCommandFailed(commandEvent: BotControlCommandRealtimeEvent) {
        if (!matchesPendingCommand(commandEvent)) {
            return
        }
        pendingCommandTracker.finish()
        refreshCurrentPlaybackMonitorUseCase()
        _effect.emit(QueueEffect.BotCommandFailed(commandEvent.type, commandEvent.errorKey))
    }

    private fun updateManualDraft(value: String) {
        _uiState.update {
            it.copy(
                addMusic = it.addMusic.copy(
                    manualDraft = value,
                    canSubmitManual = value.canSubmitMusicInput()
                )
            )
        }
    }

    private fun openDialog(dialog: QueueDialog) {
        _uiState.update { it.copy(dialog = dialog) }
    }

    private fun closeDialog() {
        searchController.clear()

        _uiState.update {
            it.copy(
                dialog = QueueDialog.None,
                playlistNameDraft = ""
            )
        }
    }

    private fun clearCommandExecution() {
        commandVersion++
        commandJob?.cancel()
        commandJob = null

        pendingCommandTracker.clear()
        commandLock.finish()
    }

    override fun onCleared() {
        commandRealtimeObserver.clear()
        playbackMonitorJob?.cancel()

        clearCommandExecution()
        searchController.cancel()

        super.onCleared()
    }

    private companion object {
        const val COMMAND_TERMINAL_EVENT_TIMEOUT = 30_000L
    }
}
