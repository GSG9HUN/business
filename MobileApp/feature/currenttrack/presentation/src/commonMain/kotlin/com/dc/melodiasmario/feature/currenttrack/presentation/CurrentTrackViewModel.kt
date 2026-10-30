package com.dc.melodiasmario.feature.currenttrack.presentation

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.commonui.music.validation.canSubmitMusicInput
import com.dc.melodiasmario.core.commonui.playbacknotification.PlaybackNotificationController
import com.dc.melodiasmario.core.domain.currenttrack.usecase.NextTrackUseCase
import com.dc.melodiasmario.core.domain.currenttrack.usecase.PauseUseCase
import com.dc.melodiasmario.core.domain.currenttrack.usecase.PlayUseCase
import com.dc.melodiasmario.core.domain.currenttrack.usecase.PreviousTrackUseCase
import com.dc.melodiasmario.core.domain.currenttrack.usecase.SetRepeatModeUseCase
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
import com.dc.melodiasmario.feature.currenttrack.presentation.command.CurrentTrackCommandHandler
import com.dc.melodiasmario.feature.currenttrack.presentation.dialog.CurrentTrackDialog
import com.dc.melodiasmario.feature.currenttrack.presentation.state.CurrentTrackPlaybackUiState
import com.dc.melodiasmario.feature.currenttrack.presentation.state.CurrentTrackUiState
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
 * Coordinates current track events, observable UI state, and one-time effects.
 */
@KoinViewModel
class CurrentTrackViewModel(
    private val getCurrentUserUseCase: GetCurrentUserUseCase,
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
    private val getMusicSearchCapabilitiesUseCase: GetMusicSearchCapabilitiesUseCase,
    private val searchMusicUseCase: SearchMusicUseCase,
    private val observeGuildRealtimeUseCase: ObserveGuildRealtimeUseCase,
    private val observeCurrentPlaybackMonitorUseCase: ObserveCurrentPlaybackMonitorUseCase,
    private val startCurrentPlaybackMonitorUseCase: StartCurrentPlaybackMonitorUseCase,
    private val refreshCurrentPlaybackMonitorUseCase: RefreshCurrentPlaybackMonitorUseCase,
    private val playbackNotificationController: PlaybackNotificationController
) : ViewModel() {
    private val _uiState = MutableStateFlow(value = CurrentTrackUiState())

    /**
     * Read-only presentation state observed by UI consumers.
     */
    val uiState: StateFlow<CurrentTrackUiState> = _uiState.onStart {
        playbackMonitorObserver.start()
    }
        .stateIn(
            scope = viewModelScope,
            started = WhileSubscribed(stopTimeoutMillis = 5_000),
            initialValue = CurrentTrackUiState()
        )
    private val playbackMonitorObserver = PlaybackMonitorObserver(
        coroutineScope = viewModelScope,
        observePlayback = observeCurrentPlaybackMonitorUseCase::invoke,
        onPlayback = ::updatePlayback
    )
    private val events = MutableSharedFlow<CurrentTrackEvent>(extraBufferCapacity = 64)
    private val _effect = MutableSharedFlow<CurrentTrackEffect>()

    /**
     * One-time presentation outcomes observed by UI consumers.
     */
    val effect: SharedFlow<CurrentTrackEffect> = _effect.asSharedFlow()
    private var currentGuildId: String? = null
    private var currentUserId: String? = null
    private var commandJob: Job? = null
    private var commandVersion = 0L

    private val commandLock = CommandLock(uiState = _uiState)
    private val botCommandCoordinator = BotCommandCoordinator(
        coroutineScope = viewModelScope,
        observeGuildRealtime = observeGuildRealtimeUseCase::invoke,
        currentUserIdProvider = { currentUserId }
    )
    private val currentTrackCommandHandler = CurrentTrackCommandHandler(
        addToQueueUseCase = addToQueueUseCase,
        clearQueueUseCase = clearQueueUseCase,
        nextTrackUseCase = nextTrackUseCase,
        previousTrackUseCase = previousTrackUseCase,
        playUseCase = playUseCase,
        pauseUseCase = pauseUseCase,
        setRepeatModeUseCase = setRepeatModeUseCase,
        removeFromQueueUseCase = removeFromQueueUseCase,
        moveTrackToIndexUseCase = moveTrackToIndexUseCase,
        shuffleQueueUseCase = shuffleQueueUseCase,
        uiState = _uiState,
        effects = _effect,
        currentGuildIdProvider = { currentGuildId },
        onTrackPendingCommand = { command ->
            botCommandCoordinator.handleCompletion(
                command = command,
                onSucceeded = { _effect.emit(value = CurrentTrackEffect.BotCommandSucceeded(commandType = command.type)) },
                onFailed = { errorKey ->
                    _effect.emit(
                        value = CurrentTrackEffect.BotCommandFailed(
                            commandType = command.type,
                            errorKey = errorKey
                        )
                    )
                },
                onTimedOut = { },
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
        failureEffect = CurrentTrackEffect.SearchFailed,
        coroutineScope = viewModelScope,
        currentGuildIdProvider = { currentGuildId }
    )

    init {
        collectEvents()
    }

    /**
     * Submits a user or lifecycle event to the feature event-processing flow.
     */
    fun onEvent(event: CurrentTrackEvent): Unit {
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

    private suspend fun handleEvent(event: CurrentTrackEvent) {
        when (event) {
            is CurrentTrackEvent.LoadCurrentTrack -> {
                if (currentGuildId != event.guildId) {
                    clearCommandExecution()
                    musicSearchController.resetForGuildChange(newGuildId = event.guildId, currentGuildId = currentGuildId)
                    currentGuildId = event.guildId
                    currentUserId = null
                    _uiState.value = CurrentTrackUiState(isLoading = true)
                }

                botCommandCoordinator.start(guildId = event.guildId)
                startCurrentPlaybackMonitorUseCase(guildId = event.guildId)
                playbackNotificationController.start(guildId = event.guildId)
                updatePlayback(playbackState = observeCurrentPlaybackMonitorUseCase().value)
                loadCurrentUser()
            }

            CurrentTrackEvent.SyncCurrentTrack -> refreshCurrentPlaybackMonitorUseCase()
            is CurrentTrackEvent.AddToQueueDraftChanged -> updateAddToQueueDraft(value = event.value)
            CurrentTrackEvent.RefreshClicked -> {
                _uiState.update { currentUiState -> currentUiState.copy(isLoading = true, errorMessage = null) }
                refreshCurrentPlaybackMonitorUseCase()
            }

            CurrentTrackEvent.GuildClicked -> _effect.emit(value = CurrentTrackEffect.NavigateToGuildSelector)
            CurrentTrackEvent.ProfileClicked -> _effect.emit(value = CurrentTrackEffect.NavigateToProfile)
            CurrentTrackEvent.AddToQueueClicked -> {
                openDialog(dialog = CurrentTrackDialog.AddToQueue)
                musicSearchController.loadCapabilities()
            }

            CurrentTrackEvent.AddToQueueConfirmed -> runCommandIfIdle { handleCommandEvent(event = event) }
            CurrentTrackEvent.DialogDismissed -> closeDialog()
            is CurrentTrackEvent.RemoveFromQueueClicked -> runCommandIfIdle {
                handleCommandEvent(event = event)
            }

            CurrentTrackEvent.ClearQueueClicked -> runCommandIfIdle { handleCommandEvent(event = event) }
            CurrentTrackEvent.MoreClicked -> openDialog(dialog = CurrentTrackDialog.MoreActions)
            CurrentTrackEvent.NextClicked -> runCommandIfIdle { handleCommandEvent(event = event) }
            CurrentTrackEvent.PreviousClicked -> runCommandIfIdle { handleCommandEvent(event = event) }
            CurrentTrackEvent.RepeatClicked -> runCommandIfIdle { handleCommandEvent(event = event) }
            CurrentTrackEvent.ShuffleClicked -> runCommandIfIdle { handleCommandEvent(event = event) }
            CurrentTrackEvent.PlayPauseClicked -> runCommandIfIdle { handleCommandEvent(event = event) }
            is CurrentTrackEvent.MoveToIndexClicked -> runCommandIfIdle { handleCommandEvent(event = event) }
            is CurrentTrackEvent.SearchQueryChanged -> musicSearchController.onQueryChanged(query = event.query)
            is CurrentTrackEvent.SearchProviderChanged -> musicSearchController.onProviderChanged(providerId = event.providerId)
            is CurrentTrackEvent.SearchKindChanged -> musicSearchController.onKindChanged(kind = event.kind)
            CurrentTrackEvent.SearchSubmitted -> musicSearchController.submit()
            CurrentTrackEvent.SearchRetryClicked -> musicSearchController.retry()
            CurrentTrackEvent.SearchNextPageRequested -> musicSearchController.loadNextPage()
            is CurrentTrackEvent.SearchResultClicked -> musicSearchController.selectResult(resultId = event.resultId)
            CurrentTrackEvent.SelectedSearchResultAddClicked -> runCommandIfIdle {
                handleCommandEvent(event = event)
            }

            CurrentTrackEvent.SearchCapabilitiesRetryClicked -> musicSearchController.loadCapabilities()
            is CurrentTrackEvent.AddMusicModeChanged -> musicSearchController.updateMode(mode = event.mode)
        }
    }

    private suspend fun handleCommandEvent(event: CurrentTrackEvent) {
        currentTrackCommandHandler.handle(event)
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

    private suspend fun loadCurrentUser() {
        getCurrentUserUseCase().collect { result ->

            if (result is Resource.Success) {
                currentUserId = result.data.id
                _uiState.update { currentUiState ->
                    currentUiState.copy(
                        header = currentUiState.header.copy(
                            profileName = result.data.displayName,
                            profileAvatarUrl = result.data.avatarUrl
                        )
                    )
                }
            }
        }
    }

    private fun updateAddToQueueDraft(value: String) {
        _uiState.update { currentUiState ->
            currentUiState.copy(
                addMusic = currentUiState.addMusic.copy(
                    manualDraft = value,
                    canSubmitManual = value.canSubmitMusicInput()
                )
            )
        }
    }

    private fun openDialog(dialog: CurrentTrackDialog) {
        _uiState.update { currentUiState -> currentUiState.copy(dialog = dialog) }
    }

    private fun closeDialog() {
        musicSearchController.clear()
        _uiState.update { currentUiState ->
            currentUiState.copy(
                dialog = CurrentTrackDialog.None,
                addMusic = currentUiState.addMusic.copy(
                    manualDraft = "",
                    canSubmitManual = false,
                    isManualLoading = false
                )
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

    private fun updatePlayback(playbackState: CurrentPlaybackMonitorState) {
        val guildId = currentGuildId ?: return

        if (playbackState.guildId != guildId) {
            return
        }

        val track = playbackState.currentTrack?.takeIf { currentTrack -> currentTrack.guildId == guildId }
        _uiState.update { value ->

            if (track == null) {
                value.copy(
                    isLoading = playbackState.isRefreshing,
                    errorMessage = playbackState.errorMessage,
                    header = value.header.copy(guildName = "", guildIconUrl = null),
                    playback = CurrentTrackPlaybackUiState(),
                    queue = value.queue.copy(tracks = emptyList())
                )
            } else {
                value.copy(
                    isLoading = false,
                    errorMessage = null,
                    header = value.header.copy(
                        guildName = track.guildName,
                        guildIconUrl = track.guildIconUrl,
                        guildBotStatus = track.guildBotStatus
                    ),
                    playback = value.playback.copy(
                        currentTrack = track,
                        isPlaying = track.isPlaying,
                        repeatMode = track.repeatMode
                    ),
                    queue = value.queue.copy(
                        tracks = track.queuedTracks
                    )
                )
            }
        }
    }
}
