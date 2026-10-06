package com.dc.melodiasmario.feature.currenttrack.presentation

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.common.presentation.runAction
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.domain.queue.usecase.AddToQueueUseCase
import com.dc.melodiasmario.core.domain.queue.usecase.ClearQueueUseCase
import com.dc.melodiasmario.core.domain.currenttrack.usecase.GetCurrentTrackUseCase
import com.dc.melodiasmario.core.domain.queue.usecase.MoveTrackToIndexUseCase
import com.dc.melodiasmario.core.domain.currenttrack.usecase.NextTrackUseCase
import com.dc.melodiasmario.core.domain.currenttrack.usecase.PauseUseCase
import com.dc.melodiasmario.core.domain.currenttrack.usecase.PlayUseCase
import com.dc.melodiasmario.core.domain.currenttrack.usecase.PreviousTrackUseCase
import com.dc.melodiasmario.core.domain.currenttrack.usecase.SetRepeatModeUseCase
import com.dc.melodiasmario.core.domain.queue.usecase.RemoveFromQueueUseCase
import com.dc.melodiasmario.core.domain.queue.usecase.ShuffleQueueUseCase
import com.dc.melodiasmario.core.domain.currentuser.usecase.GetCurrentUserUseCase
import com.dc.melodiasmario.core.domain.realtime.usecase.ObserveGuildRealtimeUseCase
import com.dc.melodiasmario.core.domain.realtime.usecase.ObserveRealtimeConnectionStateUseCase
import com.dc.melodiasmario.core.domain.search.usecase.GetMusicSearchCapabilitiesUseCase
import com.dc.melodiasmario.core.domain.search.usecase.SearchMusicUseCase
import com.dc.melodiasmario.core.commonui.music.validation.canSubmitMusicInput
import com.dc.melodiasmario.core.model.currenttrack.CurrentTrack
import com.dc.melodiasmario.core.model.realtime.BotControlCommandRealtimeEvent
import com.dc.melodiasmario.core.model.realtime.MobileRealtimeEventNames
import com.dc.melodiasmario.core.model.realtime.RealtimeEvent
import com.dc.melodiasmario.feature.currenttrack.presentation.command.CurrentTrackCommandHandler
import com.dc.melodiasmario.feature.currenttrack.presentation.command.CurrentTrackCommandLock
import com.dc.melodiasmario.feature.currenttrack.presentation.command.PendingBotCommandTracker
import com.dc.melodiasmario.feature.currenttrack.presentation.dialog.CurrentTrackDialog
import com.dc.melodiasmario.feature.currenttrack.presentation.realtime.CurrentTrackPollingController
import com.dc.melodiasmario.feature.currenttrack.presentation.realtime.CurrentTrackRealtimeHandler
import com.dc.melodiasmario.feature.currenttrack.presentation.search.CurrentTrackSearchController
import com.dc.melodiasmario.feature.currenttrack.presentation.state.CurrentTrackUiState
import kotlinx.coroutines.FlowPreview
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asSharedFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.debounce
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import org.koin.core.annotation.KoinViewModel
import kotlin.time.Duration.Companion.milliseconds

@KoinViewModel
class CurrentTrackViewModel(
    private val getCurrentTrackUseCase: GetCurrentTrackUseCase,
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
    private val observeGuildRealtimeUseCase: ObserveGuildRealtimeUseCase,
    private val observeRealtimeConnectionStateUseCase: ObserveRealtimeConnectionStateUseCase,
    private val getMusicSearchCapabilitiesUseCase: GetMusicSearchCapabilitiesUseCase,
    private val searchMusicUseCase: SearchMusicUseCase,
) : ViewModel() {
    private val _uiState = MutableStateFlow(CurrentTrackUiState())
    val uiState: StateFlow<CurrentTrackUiState> = _uiState.asStateFlow()
    private val events = MutableSharedFlow<CurrentTrackEvent>(extraBufferCapacity = 64)
    private val _effect = MutableSharedFlow<CurrentTrackEffect>()
    val effect = _effect.asSharedFlow()
    private val realtimeRefreshRequests = MutableSharedFlow<Unit>(extraBufferCapacity = 1)
    private var currentGuildId: String? = null
    private var currentUserId: String? = null
    private val pollingController = CurrentTrackPollingController(
        scope = viewModelScope,
        intervalMillis = CURRENT_TRACK_POOLING_FALLBACK_INTERVAL,
        onRefresh = { refresh(showLoading = false) },
    )
    private val realtimeHandler = CurrentTrackRealtimeHandler(
        scope = viewModelScope,
        observeGuildRealtimeUseCase = observeGuildRealtimeUseCase,
        observeRealtimeConnectionStateUseCase = observeRealtimeConnectionStateUseCase,
        pollingController = pollingController,
        onRealtimeEvent = ::handleRealtimeEvent,
    )
    private val commandLock = CurrentTrackCommandLock(_uiState)
    private val pendingCommandTracker = PendingBotCommandTracker(
        scope = viewModelScope,
        timeoutMillis = COMMAND_TERMINAL_EVENT_TIMEOUT,
        onTimeout = { refresh(showLoading = false) },
        onFinished = commandLock::finish,
    )
    private val commandHandler = CurrentTrackCommandHandler(
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
        state = _uiState,
        effects = _effect,
        currentGuildIdProvider = { currentGuildId },
        onTrackPendingCommand = ::trackPendingCommand,
        onCloseDialog = ::closeDialog,
    )
    private val searchController = CurrentTrackSearchController(
        getMusicSearchCapabilitiesUseCase = getMusicSearchCapabilitiesUseCase,
        searchMusicUseCase = searchMusicUseCase,
        state = _uiState,
        effects = _effect,
        scope = viewModelScope,
        currentGuildIdProvider = { currentGuildId },
    )

    init {
        collectEvents()
        collectRealtimeRefreshRequests()
    }

    fun onEvent(event: CurrentTrackEvent) {
        if (event.isCommandEvent()) {
            if (!startCommandIfIdle()) return

            viewModelScope.launch {
                try {
                    handleCommandEvent(event)
                } finally {
                    finishCommandIfNoPendingCommand()
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

    @OptIn(FlowPreview::class)
    private fun collectRealtimeRefreshRequests() {
        viewModelScope.launch {
            realtimeRefreshRequests
                .debounce(CURRENT_TRACK_REALTIME_REFRESH_DEBOUNCE.milliseconds)
                .collect {
                    refresh(showLoading = false)
                }
        }
    }

    private suspend fun handleEvent(event: CurrentTrackEvent) {
        when (event) {
            is CurrentTrackEvent.LoadCurrentTrack -> {
                searchController.resetForGuildChange(
                    newGuildId = event.guildId,
                    currentGuildId = currentGuildId,
                )
                loadCurrentUser()
                getCurrentTrack(event.guildId, showLoading = true)
                realtimeHandler.start(event.guildId)
            }

            CurrentTrackEvent.SyncCurrentTrack -> refresh(showLoading = false)
            is CurrentTrackEvent.AddToQueueDraftChanged -> updateAddToQueueDraft(event.value)
            CurrentTrackEvent.RefreshClicked -> refresh(showLoading = true)
            CurrentTrackEvent.GuildClicked -> _effect.emit(CurrentTrackEffect.NavigateToGuildSelector)
            CurrentTrackEvent.ProfileClicked -> _effect.emit(CurrentTrackEffect.NavigateToProfile)
            CurrentTrackEvent.AddToQueueClicked -> {
                openDialog(CurrentTrackDialog.AddToQueue)
                searchController.loadCapabilities()
            }

            CurrentTrackEvent.AddToQueueConfirmed -> runCommandIfIdle { handleCommandEvent(event) }
            CurrentTrackEvent.DialogDismissed -> closeDialog()
            is CurrentTrackEvent.RemoveFromQueueClicked -> runCommandIfIdle {
                handleCommandEvent(
                    event
                )
            }

            CurrentTrackEvent.ClearQueueClicked -> runCommandIfIdle { handleCommandEvent(event) }
            CurrentTrackEvent.MoreClicked -> openDialog(CurrentTrackDialog.MoreActions)
            CurrentTrackEvent.NextClicked -> runCommandIfIdle { handleCommandEvent(event) }
            CurrentTrackEvent.PreviousClicked -> runCommandIfIdle { handleCommandEvent(event) }
            CurrentTrackEvent.RepeatClicked -> runCommandIfIdle { handleCommandEvent(event) }
            CurrentTrackEvent.ShuffleClicked -> runCommandIfIdle { handleCommandEvent(event) }
            CurrentTrackEvent.PlayPauseClicked -> runCommandIfIdle { handleCommandEvent(event) }
            is CurrentTrackEvent.MoveToIndexClicked -> runCommandIfIdle { handleCommandEvent(event) }
            is CurrentTrackEvent.SearchQueryChanged -> searchController.onQueryChanged(event.query)
            is CurrentTrackEvent.SearchProviderChanged -> searchController.onProviderChanged(event.providerId)
            is CurrentTrackEvent.SearchKindChanged -> searchController.onKindChanged(event.kind)
            CurrentTrackEvent.SearchSubmitted -> searchController.submit()
            CurrentTrackEvent.SearchRetryClicked -> searchController.retry()
            CurrentTrackEvent.SearchNextPageRequested -> searchController.loadNextPage()
            is CurrentTrackEvent.SearchResultClicked -> searchController.selectResult(event.resultId)
            CurrentTrackEvent.SelectedSearchResultAddClicked -> runCommandIfIdle {
                handleCommandEvent(event)
            }
            CurrentTrackEvent.SearchCapabilitiesRetryClicked -> searchController.loadCapabilities()
            is CurrentTrackEvent.AddMusicModeChanged -> searchController.updateMode(event.mode)
        }
    }

    private suspend fun handleCommandEvent(event: CurrentTrackEvent) {
        commandHandler.handle(event)
    }

    private suspend fun runCommandIfIdle(action: suspend () -> Unit) {
        if (!startCommandIfIdle()) return

        try {
            action()
        } finally {
            finishCommandIfNoPendingCommand()
        }
    }

    private fun startCommandIfIdle(): Boolean {
        return commandLock.startIfIdle()
    }

    private fun finishCommand() {
        pendingCommandTracker.finish()
    }

    private fun finishCommandIfNoPendingCommand() {
        pendingCommandTracker.finishIfNoPendingCommand()
    }

    private fun trackPendingCommand(command: BotControlCommand) {
        pendingCommandTracker.track(command)
    }

    private fun CurrentTrackEvent.isCommandEvent(): Boolean {
        return this is CurrentTrackEvent.AddToQueueConfirmed ||
            this is CurrentTrackEvent.RemoveFromQueueClicked ||
            this is CurrentTrackEvent.ClearQueueClicked ||
            this is CurrentTrackEvent.NextClicked ||
            this is CurrentTrackEvent.PreviousClicked ||
            this is CurrentTrackEvent.RepeatClicked ||
            this is CurrentTrackEvent.ShuffleClicked ||
            this is CurrentTrackEvent.PlayPauseClicked ||
            this is CurrentTrackEvent.MoveToIndexClicked ||
            this is CurrentTrackEvent.SelectedSearchResultAddClicked
    }

    private suspend fun getCurrentTrack(
        guildId: String,
        showLoading: Boolean,
    ) {
        currentGuildId = guildId
        getCurrentTrackUseCase(guildId).runAction(
            state = _uiState,
            effects = _effect,
            failureEffect = CurrentTrackEffect.LoadCurrentTrackFailed,
            onLoading = if (showLoading) {
                { currentState -> currentState.loading() }
            } else {
                null
            },
            onSuccessWithData = { currentState, currentTrack ->
                currentState.currentTrackLoaded(currentTrack)
            },
            onError = { currentState, error ->
                currentState.currentTrackFailed(error)
            },
        )
    }

    private suspend fun refresh(showLoading: Boolean = false) {
        currentGuildId?.let { getCurrentTrack(it, showLoading) }
    }

    private suspend fun loadCurrentUser() {
        getCurrentUserUseCase().collect { result ->
            if (result is Resource.Success) {
                currentUserId = result.data.id
                _uiState.update {
                    it.copy(
                        header = it.header.copy(
                            profileName = result.data.displayName,
                            profileAvatarUrl = result.data.avatarUrl,
                        )
                    )
                }
            }
        }
    }

    private fun updateAddToQueueDraft(value: String) {
        _uiState.update {
            it.copy(
                addMusic = it.addMusic.copy(
                    manualDraft = value,
                    canSubmitManual = value.canSubmitMusicInput(),
                )
            )
        }
    }

    private fun openDialog(dialog: CurrentTrackDialog) {
        _uiState.update { it.copy(dialog = dialog) }
    }

    private fun closeDialog() {
        searchController.clear()
        _uiState.update {
            it.copy(
                dialog = CurrentTrackDialog.None,
                addMusic = it.addMusic.copy(
                    manualDraft = "",
                    canSubmitManual = false,
                    isManualLoading = false,
                ),
            )
        }
    }

    private fun CurrentTrackUiState.loading(): CurrentTrackUiState {
        return copy(
            isLoading = true,
            errorMessage = null,
        )
    }

    private fun CurrentTrackUiState.currentTrackLoaded(currentTrack: CurrentTrack): CurrentTrackUiState {
        return copy(
            isLoading = false,
            errorMessage = null,
            header = header.copy(
                guildName = currentTrack.guildName,
                guildIconUrl = currentTrack.guildIconUrl,
                guildBotStatus = currentTrack.guildBotStatus,
            ),
            playback = playback.copy(
                currentTrack = currentTrack,
                isPlaying = currentTrack.isPlaying,
                repeatMode = currentTrack.repeatMode,
            ),
            queue = queue.copy(
                tracks = currentTrack.queuedTracks,
            ),
        )
    }

    private fun CurrentTrackUiState.currentTrackFailed(error: Throwable): CurrentTrackUiState {
        return copy(
            isLoading = false,
            errorMessage = error.message,
        )
    }

    private suspend fun handleRealtimeEvent(event: RealtimeEvent) {
        when (event.eventName) {
            MobileRealtimeEventNames.BotControlCommandSucceeded -> {
                handleBotControlCommandSucceeded(event)
            }

            MobileRealtimeEventNames.BotControlCommandFailed,
            MobileRealtimeEventNames.BotControlCommandInterrupted -> {
                handleBotControlCommandFailed(event)
            }

            MobileRealtimeEventNames.PlaybackSnapshotChanged,
            MobileRealtimeEventNames.CurrentTrackChanged,
            MobileRealtimeEventNames.PlaybackStarted,
            MobileRealtimeEventNames.PlaybackPaused,
            MobileRealtimeEventNames.PlaybackResumed,
            MobileRealtimeEventNames.PlaybackStopped,
            MobileRealtimeEventNames.PlaybackSkipped,
            MobileRealtimeEventNames.PlaybackPreviousStarted,
            MobileRealtimeEventNames.PlaybackLoadFailed,
            MobileRealtimeEventNames.RepeatModeChanged,
            MobileRealtimeEventNames.QueueSnapshotChanged,
            MobileRealtimeEventNames.QueueItemAdded,
            MobileRealtimeEventNames.QueueItemsAdded,
            MobileRealtimeEventNames.QueueItemRemoved,
            MobileRealtimeEventNames.QueueCleared,
            MobileRealtimeEventNames.QueueShuffled,
            MobileRealtimeEventNames.QueueItemMoved,
            MobileRealtimeEventNames.QueueItemClaimed,
            MobileRealtimeEventNames.QueueCompacted,
            MobileRealtimeEventNames.RepeatListSnapshotChanged,
            MobileRealtimeEventNames.GuildBotStatusChanged,
            MobileRealtimeEventNames.BotJoinedVoiceChannel,
            MobileRealtimeEventNames.BotLeftVoiceChannel,
            MobileRealtimeEventNames.PlaybackPositionChanged,
            MobileRealtimeEventNames.BotVoiceUserCountChanged -> {
                scheduleRealtimeRefresh()
            }
        }
    }

    private suspend fun handleBotControlCommandSucceeded(event: RealtimeEvent) {
        val commandEvent = event as? BotControlCommandRealtimeEvent ?: return
        if (!pendingCommandTracker.matches(commandEvent, currentUserId)) return

        _effect.emit(CurrentTrackEffect.BotCommandSucceeded(commandEvent.type))
        finishCommand()
        scheduleRealtimeRefresh()
    }

    private suspend fun handleBotControlCommandFailed(event: RealtimeEvent) {
        val commandEvent = event as? BotControlCommandRealtimeEvent ?: return
        if (!pendingCommandTracker.matches(commandEvent, currentUserId)) return

        _effect.emit(
            CurrentTrackEffect.BotCommandFailed(
                commandType = commandEvent.type,
                errorKey = commandEvent.errorKey,
            )
        )
        finishCommand()
        scheduleRealtimeRefresh()
    }

    private fun scheduleRealtimeRefresh() {
        realtimeRefreshRequests.tryEmit(Unit)
    }

    override fun onCleared() {
        realtimeHandler.clear()
        pendingCommandTracker.clear()
        searchController.cancel()
        super.onCleared()
    }

    private companion object {
        const val CURRENT_TRACK_POOLING_FALLBACK_INTERVAL = 15_000L
        const val CURRENT_TRACK_REALTIME_REFRESH_DEBOUNCE = 500L
        const val COMMAND_TERMINAL_EVENT_TIMEOUT = 30_000L
    }
}
