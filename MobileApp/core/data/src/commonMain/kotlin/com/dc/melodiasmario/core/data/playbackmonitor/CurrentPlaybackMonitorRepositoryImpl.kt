package com.dc.melodiasmario.core.data.playbackmonitor

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.currenttrack.usecase.GetCurrentTrackUseCase
import com.dc.melodiasmario.core.domain.playbackmonitor.CurrentPlaybackMonitorRepository
import com.dc.melodiasmario.core.domain.realtime.usecase.ObserveGuildRealtimeUseCase
import com.dc.melodiasmario.core.domain.realtime.usecase.ObserveRealtimeConnectionStateUseCase
import com.dc.melodiasmario.core.model.playbackmonitor.CurrentPlaybackMonitorState
import com.dc.melodiasmario.core.model.realtime.MobileRealtimeEventNames
import com.dc.melodiasmario.core.model.realtime.RealtimeConnectionState
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Job
import kotlinx.coroutines.SupervisorJob
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.isActive
import kotlinx.coroutines.launch
import kotlinx.coroutines.currentCoroutineContext
import org.koin.core.annotation.Single
import kotlin.time.Clock
import kotlin.time.Duration.Companion.milliseconds

@Single(binds = [CurrentPlaybackMonitorRepository::class])
class CurrentPlaybackMonitorRepositoryImpl(
    private val getCurrentTrackUseCase: GetCurrentTrackUseCase,
    private val observeGuildRealtimeUseCase: ObserveGuildRealtimeUseCase,
    private val observeRealtimeConnectionStateUseCase: ObserveRealtimeConnectionStateUseCase,
) : CurrentPlaybackMonitorRepository {
    private val scope = CoroutineScope(SupervisorJob())
    private val _state = MutableStateFlow(CurrentPlaybackMonitorState())
    override val state: StateFlow<CurrentPlaybackMonitorState> = _state.asStateFlow()

    private var monitorJob: Job? = null
    private var currentGuildId: String? = null

    override fun start(guildId: String) {
        if (currentGuildId == guildId && monitorJob?.isActive == true) return

        monitorJob?.cancel()
        currentGuildId = guildId
        _state.update { it.copy(guildId = guildId, errorMessage = null) }

        monitorJob = scope.launch {
            launch { observeRealtime(guildId) }
            launch { observeConnection() }
            launch { pollFallback(guildId) }
            refreshInternal(guildId)
        }
    }

    override fun stop() {
        monitorJob?.cancel()
        monitorJob = null
        currentGuildId = null
        _state.value = CurrentPlaybackMonitorState()
    }

    override fun refresh() {
        val guildId = currentGuildId ?: return
        scope.launch { refreshInternal(guildId) }
    }

    private suspend fun observeRealtime(guildId: String) {
        observeGuildRealtimeUseCase(guildId).collect { event ->
            if (event.eventName.shouldRefreshPlayback()) {
                refreshInternal(guildId)
            }
        }
    }

    private suspend fun observeConnection() {
        observeRealtimeConnectionStateUseCase().collect { connectionState ->
            _state.update {
                it.copy(isConnected = connectionState == RealtimeConnectionState.Connected)
            }
        }
    }

    private suspend fun pollFallback(guildId: String) {
        while (currentCoroutineContext().isActive) {
            delay(15_000.milliseconds)
            refreshInternal(guildId)
        }
    }

    private suspend fun refreshInternal(guildId: String) {
        getCurrentTrackUseCase(guildId).collect { result ->
            when (result) {
                Resource.Loading -> _state.update { it.copy(isRefreshing = true) }

                is Resource.Success -> _state.update {
                    it.copy(
                        guildId = guildId,
                        currentTrack = result.data,
                        isRefreshing = false,
                        lastUpdatedAtMillis = Clock.System.now().toEpochMilliseconds(),
                        errorMessage = null,
                    )
                }

                is Resource.Error -> _state.update {
                    it.copy(
                        isRefreshing = false,
                        errorMessage = result.error.message,
                    )
                }
            }
        }
    }

    private fun String.shouldRefreshPlayback(): Boolean {
        return this in setOf(
            MobileRealtimeEventNames.PlaybackSnapshotChanged,
            MobileRealtimeEventNames.CurrentTrackChanged,
            MobileRealtimeEventNames.PlaybackStarted,
            MobileRealtimeEventNames.PlaybackPaused,
            MobileRealtimeEventNames.PlaybackResumed,
            MobileRealtimeEventNames.PlaybackStopped,
            MobileRealtimeEventNames.PlaybackSkipped,
            MobileRealtimeEventNames.PlaybackPreviousStarted,
            MobileRealtimeEventNames.RepeatModeChanged,
            MobileRealtimeEventNames.PlaybackPositionChanged,
            MobileRealtimeEventNames.PlaybackLoadFailed,
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
            MobileRealtimeEventNames.BotVoiceUserCountChanged,
        )
    }
}
