package com.dc.melodiasmario.feature.currenttrack.presentation

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.common.presentation.runAction
import com.dc.melodiasmario.core.domain.queue.usecase.AddToQueueUseCase
import com.dc.melodiasmario.core.domain.queue.usecase.ClearQueueUseCase
import com.dc.melodiasmario.core.domain.currenttrack.usecase.GetCurrentTrackUseCase
import com.dc.melodiasmario.core.domain.queue.usecase.MoveTrackDownUseCase
import com.dc.melodiasmario.core.domain.queue.usecase.MoveTrackUpUseCase
import com.dc.melodiasmario.core.domain.currenttrack.usecase.NextTrackUseCase
import com.dc.melodiasmario.core.domain.currenttrack.usecase.PauseUseCase
import com.dc.melodiasmario.core.domain.currenttrack.usecase.PlayUseCase
import com.dc.melodiasmario.core.domain.currenttrack.usecase.PreviousTrackUseCase
import com.dc.melodiasmario.core.domain.currenttrack.usecase.SetRepeatModeUseCase
import com.dc.melodiasmario.core.domain.queue.usecase.RemoveFromQueueUseCase
import com.dc.melodiasmario.core.domain.queue.usecase.ShuffleQueueUseCase
import com.dc.melodiasmario.core.domain.currentuser.usecase.GetCurrentUserUseCase
import com.dc.melodiasmario.core.model.currenttrack.CurrentTrack
import com.dc.melodiasmario.core.model.currenttrack.RepeatMode
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asSharedFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import org.koin.core.annotation.KoinViewModel

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
    private val moveTrackUpUseCase: MoveTrackUpUseCase,
    private val moveTrackDownUseCase: MoveTrackDownUseCase,
    private val shuffleQueueUseCase: ShuffleQueueUseCase,
) : ViewModel() {
    private val _uiState = MutableStateFlow(CurrentTrackUiState())
    val uiState: StateFlow<CurrentTrackUiState> = _uiState.asStateFlow()
    private val events = MutableSharedFlow<CurrentTrackEvent>(extraBufferCapacity = 64)
    private val _effect = MutableSharedFlow<CurrentTrackEffect>()
    val effect = _effect.asSharedFlow()
    private var currentGuildId: String? = null

    init {
        collectEvents()
    }

    fun onEvent(event: CurrentTrackEvent) {
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

    private suspend fun handleEvent(event: CurrentTrackEvent) {
        when (event) {
            is CurrentTrackEvent.LoadCurrentTrack -> {
                loadCurrentUser()
                getCurrentTrack(event.guildId, showLoading = true)
            }

            CurrentTrackEvent.SyncCurrentTrack -> refresh(showLoading = false)
            is CurrentTrackEvent.AddToQueueDraftChanged -> updateAddToQueueDraft(event.value)
            CurrentTrackEvent.RefreshClicked -> refresh(showLoading = true)
            CurrentTrackEvent.GuildClicked -> _effect.emit(CurrentTrackEffect.NavigateToGuildSelector)
            CurrentTrackEvent.ProfileClicked -> _effect.emit(CurrentTrackEffect.NavigateToProfile)
            CurrentTrackEvent.AddToQueueClicked -> openDialog(CurrentTrackDialog.AddToQueue)
            CurrentTrackEvent.AddToQueueConfirmed -> addToQueue()
            CurrentTrackEvent.DialogDismissed -> closeDialog()
            is CurrentTrackEvent.RemoveFromQueueClicked -> removeFromQueue(event.index)
            CurrentTrackEvent.ClearQueueClicked -> clearQueue()
            CurrentTrackEvent.MoreClicked -> openDialog(CurrentTrackDialog.MoreActions)
            CurrentTrackEvent.NextClicked -> nextTrack()
            CurrentTrackEvent.PreviousClicked -> previousTrack()
            CurrentTrackEvent.RepeatClicked -> repeat()
            CurrentTrackEvent.ShuffleClicked -> shuffleQueue()
            CurrentTrackEvent.PlayPauseClicked -> playPause()
            is CurrentTrackEvent.MoveUpClicked -> moveTrackUp(event.index)
            is CurrentTrackEvent.MoveDownClicked -> moveTrackDown(event.index)
        }
    }

    private suspend fun getCurrentTrack(
        guildId: String,
        showLoading: Boolean,
    ) {
        currentGuildId = guildId
        getCurrentTrackUseCase(guildId).collect { result ->
            when (result) {
                Resource.Loading -> if (showLoading) onLoading()
                is Resource.Success -> onGetCurrentTrackSuccess(result.data)
                is Resource.Error -> onGetCurrentTrackError(result.error)
            }
        }
    }

    private suspend fun refresh(showLoading: Boolean = false) {
        currentGuildId?.let { getCurrentTrack(it, showLoading) }
    }

    private suspend fun loadCurrentUser() {
        getCurrentUserUseCase().collect { result ->
            if (result is Resource.Success) {
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
            it.copy(addToQueue = it.addToQueue.copy(draft = value))
        }
    }

    private suspend fun addToQueue() {
        val guildId = currentGuildId ?: return
        val urlOrQuery = _uiState.value.addToQueue.draft.trim()
        if (!_uiState.value.addToQueue.canSubmit) return

        addToQueueUseCase(guildId, urlOrQuery).runAction(
            state = _uiState,
            effects = _effect,
            successEffect = CurrentTrackEffect.AddedToQueue,
            failureEffect = CurrentTrackEffect.AddToQueueFailed,
            onLoading = {
                it.copy(addToQueue = it.addToQueue.copy(isLoading = true))
            },
            onSuccess = {
                it.copy(addToQueue = it.addToQueue.copy(isLoading = false))
            },
            onError = { state, _ ->
                state.copy(addToQueue = state.addToQueue.copy(isLoading = false))
            },
            afterSuccess = {
                closeDialog()
                refresh()
            },
        )
    }

    private suspend fun clearQueue() {
        val guildId = currentGuildId ?: return
        closeDialog()
        clearQueueUseCase(guildId).runAction(
            state = _uiState,
            effects = _effect,
            successEffect = CurrentTrackEffect.ClearedQueue,
            failureEffect = CurrentTrackEffect.ClearQueueFailed,
            onLoading = {
                it.copy(actions = it.actions.copy(isClearQueueLoading = true))
            },
            onSuccess = {
                it.copy(actions = it.actions.copy(isClearQueueLoading = false))
            },
            onError = { state, _ ->
                state.copy(actions = state.actions.copy(isClearQueueLoading = false))
            },
            afterSuccess = {
                refresh()
            },
        )
    }

    private suspend fun removeFromQueue(index: Int) {
        val guildId = currentGuildId ?: return
        val trackNumber = (index + 1).toString()
        removeFromQueueUseCase(guildId, trackNumber).runAction(
            state = _uiState,
            effects = _effect,
            successEffect = CurrentTrackEffect.RemovedFromQueue,
            failureEffect = CurrentTrackEffect.RemoveFromQueueFailed,
            afterSuccess = {
                refresh()
            },
        )
    }

    private suspend fun moveTrackUp(index: Int) {
        val guildId = currentGuildId ?: return
        moveTrackUpUseCase(guildId, index).runAction(
            state = _uiState,
            effects = _effect,
            successEffect = CurrentTrackEffect.MovedUpInQueue,
            failureEffect = CurrentTrackEffect.MoveUpInQueueFailed,
            afterSuccess = {
                refresh()
            },
        )
    }

    private suspend fun moveTrackDown(index: Int) {
        val guildId = currentGuildId ?: return
        moveTrackDownUseCase(guildId, index).runAction(
            state = _uiState,
            effects = _effect,
            successEffect = CurrentTrackEffect.MovedDownInQueue,
            failureEffect = CurrentTrackEffect.MoveDownInQueueFailed,
            afterSuccess = {
                refresh()
            },
        )
    }

    private suspend fun nextTrack() {
        val guildId = currentGuildId ?: return
        nextTrackUseCase(guildId).runAction(
            state = _uiState,
            effects = _effect,
            failureEffect = CurrentTrackEffect.NextTrackFailed,
            afterSuccess = {
                refresh()
            },
        )
    }

    private suspend fun previousTrack() {
        val guildId = currentGuildId ?: return
        previousTrackUseCase(guildId).runAction(
            state = _uiState,
            effects = _effect,
            failureEffect = CurrentTrackEffect.PreviousTrackFailed,
            afterSuccess = {
                refresh()
            },
        )
    }

    private suspend fun repeat() {
        val guildId = currentGuildId ?: return
        val nextRepeatMode = _uiState.value.playback.repeatMode.next()
        setRepeatModeUseCase(guildId, nextRepeatMode).runAction(
            state = _uiState,
            effects = _effect,
            successEffect = CurrentTrackEffect.RepeatModeChanged,
            failureEffect = CurrentTrackEffect.RepeatModeChangeFailed,
            afterSuccess = {
                refresh()
            },
        )
    }

    private fun RepeatMode.next() = when (this) {
        RepeatMode.NONE -> RepeatMode.ONE
        RepeatMode.ONE -> RepeatMode.ALL
        RepeatMode.ALL -> RepeatMode.NONE
    }

    private suspend fun shuffleQueue() {
        val guildId = currentGuildId ?: return
        closeDialog()
        shuffleQueueUseCase(guildId).runAction(
            state = _uiState,
            effects = _effect,
            successEffect = CurrentTrackEffect.QueueShuffled,
            failureEffect = CurrentTrackEffect.ShuffleQueueFailed,
            onLoading = {
                it.copy(actions = it.actions.copy(isShuffleQueueLoading = true))
            },
            onSuccess = {
                it.copy(actions = it.actions.copy(isShuffleQueueLoading = false))
            },
            onError = { state, _ ->
                state.copy(actions = state.actions.copy(isShuffleQueueLoading = false))
            },
            afterSuccess = {
                refresh()
            },
        )
    }

    private suspend fun playPause() {
        val guildId = currentGuildId ?: return
        val action = if (_uiState.value.playback.isPlaying) {
            pauseUseCase(guildId)
        } else {
            playUseCase(guildId)
        }
        action.runAction(
            state = _uiState,
            effects = _effect,
            successEffect = CurrentTrackEffect.PlayPauseToggled,
            failureEffect = CurrentTrackEffect.PlayPauseToggleFailed,
            onLoading = {
                it.copy(actions = it.actions.copy(isPlayPauseLoading = true))
            },
            onSuccess = {
                it.copy(actions = it.actions.copy(isPlayPauseLoading = false))
            },
            onError = { state, _ ->
                state.copy(actions = state.actions.copy(isPlayPauseLoading = false))
            },
            afterSuccess = {
                refresh()
            },
        )
    }

    private fun openDialog(dialog: CurrentTrackDialog) {
        _uiState.update { it.copy(dialog = dialog) }
    }
    private fun closeDialog() {
        _uiState.update {
            it.copy(
                dialog = CurrentTrackDialog.None,
                addToQueue = it.addToQueue.copy(draft = ""),
            )
        }
    }
    private fun onLoading() {
        _uiState.update {
            it.copy(
                isLoading = true,
                errorMessage = null,
            )
        }
    }

    private fun onGetCurrentTrackSuccess(currentTrack: CurrentTrack) {
        _uiState.update {
            it.copy(
                isLoading = false,
                errorMessage = null,
                header = it.header.copy(
                    guildName = currentTrack.guildName,
                    guildIconUrl = currentTrack.guildIconUrl,
                    guildBotStatus = currentTrack.guildBotStatus,
                ),
                playback = it.playback.copy(
                    currentTrack = currentTrack,
                    isPlaying = currentTrack.isPlaying,
                    repeatMode = currentTrack.repeatMode,
                ),
                queue = it.queue.copy(
                    tracks = currentTrack.queuedTracks,
                ),
            )
        }
    }

    private suspend fun onGetCurrentTrackError(error: Throwable) {
        _uiState.update {
            it.copy(
                isLoading = false,
                errorMessage = error.message,
            )
        }
        _effect.emit(CurrentTrackEffect.LoadCurrentTrackFailed)
    }
}
