package com.dc.melodiasmario.feature.queue.presentation

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.dc.melodiasmario.core.commonui.music.validation.canSubmitMusicInput
import com.dc.melodiasmario.core.domain.playbackmonitor.usecase.ObserveCurrentPlaybackMonitorUseCase
import com.dc.melodiasmario.core.domain.playbackmonitor.usecase.RefreshCurrentPlaybackMonitorUseCase
import com.dc.melodiasmario.core.domain.playbackmonitor.usecase.StartCurrentPlaybackMonitorUseCase
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.SharingStarted
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.onStart
import kotlinx.coroutines.flow.stateIn
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import org.koin.core.annotation.KoinViewModel

@KoinViewModel
class QueueViewModel(
    private val observeCurrentPlaybackMonitorUseCase: ObserveCurrentPlaybackMonitorUseCase,
    private val startCurrentPlaybackMonitorUseCase: StartCurrentPlaybackMonitorUseCase,
    private val refreshCurrentPlaybackMonitorUseCase: RefreshCurrentPlaybackMonitorUseCase,
) : ViewModel() {
    private val _uiState = MutableStateFlow(QueueUiState())
    val uiState: StateFlow<QueueUiState> = _uiState.onStart { collectPlaybackMonitor() }.stateIn(
        scope = viewModelScope,
        started = SharingStarted.WhileSubscribed(5_000),
        initialValue = QueueUiState(),
    )

    init {
        collectPlaybackMonitor()
    }

    fun onEvent(event: QueueEvent) {
        when (event) {
            is QueueEvent.LoadQueue -> {
                _uiState.update {
                    it.copy(
                        guildId = event.guildId,
                        isLoading = true,
                        errorMessage = null,
                    )
                }
                startCurrentPlaybackMonitorUseCase(event.guildId)
            }

            QueueEvent.RefreshClicked -> {
                _uiState.update { it.copy(isLoading = true, errorMessage = null) }
                refreshCurrentPlaybackMonitorUseCase()
            }

            QueueEvent.AddMusicClicked -> openDialog(QueueDialog.AddMusic)
            QueueEvent.MoreClicked -> openDialog(QueueDialog.MoreActions)
            QueueEvent.DialogDismissed -> closeDialog()
            is QueueEvent.ManualDraftChanged -> updateManualDraft(event.value)
            is QueueEvent.AddMusicModeChanged -> _uiState.update {
                it.copy(addMusic = it.addMusic.copy(mode = event.mode))
            }

            is QueueEvent.MoveTrackClicked -> moveTrack(
                fromIndex = event.fromIndex,
                toIndex = event.toIndex,
            )

            QueueEvent.ClearQueueClicked,
            QueueEvent.ShuffleQueueClicked,
            QueueEvent.MakePlaylistFromQueueClicked,
            is QueueEvent.RemoveTrackClicked,
            QueueEvent.ManualAddClicked,
            is QueueEvent.SearchQueryChanged,
            QueueEvent.SearchSubmitted,
            is QueueEvent.SearchProviderChanged,
            is QueueEvent.SearchKindChanged,
            is QueueEvent.SearchResultClicked,
            QueueEvent.SelectedSearchResultAddClicked,
            QueueEvent.SearchNextPageRequested,
            QueueEvent.SearchRetryClicked,
            QueueEvent.SearchCapabilitiesRetryClicked -> Unit
        }
    }

    private fun moveTrack(
        fromIndex: Int,
        toIndex: Int,
    ) {
        _uiState.update { state ->
            if (fromIndex !in state.tracks.indices || toIndex !in state.tracks.indices) {
                return@update state
            }

            val nextTracks = state.tracks.toMutableList()
            val movedTrack = nextTracks.removeAt(fromIndex)
            nextTracks.add(toIndex, movedTrack)

            state.copy(tracks = nextTracks)
        }
    }

    private fun collectPlaybackMonitor() {
        viewModelScope.launch {
            observeCurrentPlaybackMonitorUseCase().collect { playbackState ->
                val currentTrack = playbackState.currentTrack

                _uiState.update {
                    if (currentTrack == null) {
                        it.copy(
                            isLoading = playbackState.isRefreshing,
                            errorMessage = playbackState.errorMessage,
                        )
                    } else {
                        it.copy(
                            isLoading = false,
                            errorMessage = null,
                            guildId = playbackState.guildId,
                            guildName = currentTrack.guildName,
                            guildIconUrl = currentTrack.guildIconUrl,
                            tracks = currentTrack.queuedTracks,
                        )
                    }
                }
            }
        }
    }

    private fun openDialog(dialog: QueueDialog) {
        _uiState.update { it.copy(dialog = dialog) }
    }

    private fun closeDialog() {
        _uiState.update {
            it.copy(
                dialog = QueueDialog.None,
                addMusic = it.addMusic.copy(
                    manualDraft = "",
                    canSubmitManual = false,
                ),
            )
        }
    }

    private fun updateManualDraft(value: String) {
        _uiState.update {
            it.copy(
                addMusic = it.addMusic.copy(
                    manualDraft = value,
                    canSubmitManual = value.canSubmitMusicInput(),
                )
            )
        }
    }
}
