package com.dc.melodiasmario.core.data.playbackmonitor

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.currenttrack.usecase.GetCurrentTrackUseCase
import com.dc.melodiasmario.core.domain.playbackmonitor.CurrentPlaybackMonitorRepository
import com.dc.melodiasmario.core.domain.realtime.usecase.ObserveGuildRealtimeUseCase
import com.dc.melodiasmario.core.domain.realtime.usecase.ObserveRealtimeConnectionStateUseCase
import com.dc.melodiasmario.core.model.playbackmonitor.CurrentPlaybackMonitorState
import com.dc.melodiasmario.core.model.realtime.MobileRealtimeEventNames
import com.dc.melodiasmario.core.model.realtime.RealtimeConnectionState
import kotlin.time.Clock
import kotlin.time.Duration.Companion.milliseconds
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Job
import kotlinx.coroutines.SupervisorJob
import kotlinx.coroutines.currentCoroutineContext
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.isActive
import kotlinx.coroutines.launch
import org.koin.core.annotation.Single

/**
 * Implements current playback monitor data access using the injected data sources.
 */
@Single(binds = [CurrentPlaybackMonitorRepository::class])
class CurrentPlaybackMonitorRepositoryImpl(
    private val getCurrentTrackUseCase: GetCurrentTrackUseCase,
    private val observeGuildRealtimeUseCase: ObserveGuildRealtimeUseCase,
    private val observeRealtimeConnectionStateUseCase: ObserveRealtimeConnectionStateUseCase
) : CurrentPlaybackMonitorRepository {
    private val coroutineScope = CoroutineScope(context = SupervisorJob())
    private val _playbackMonitorState = MutableStateFlow(value = CurrentPlaybackMonitorState())
    override val playbackMonitorState: StateFlow<CurrentPlaybackMonitorState> = _playbackMonitorState.asStateFlow()

    private var monitorJob: Job? = null
    private var currentGuildId: String? = null

    override fun start(guildId: String): Unit {
        if (currentGuildId == guildId && monitorJob?.isActive == true) {
            return
        }

        monitorJob?.cancel()
        currentGuildId = guildId
        _playbackMonitorState.value = CurrentPlaybackMonitorState(guildId = guildId, isRefreshing = true)

        monitorJob = coroutineScope.launch {
            launch { observeRealtime(guildId = guildId) }
            launch { observeConnection() }
            launch { pollFallback(guildId = guildId) }
            refreshInternal(guildId = guildId)
        }
    }

    override fun refresh(): Unit {
        val guildId = currentGuildId ?: return
        coroutineScope.launch { refreshInternal(guildId = guildId) }
    }

    private suspend fun observeRealtime(guildId: String) {
        observeGuildRealtimeUseCase(guildId = guildId).collect { event ->

            if (event.eventName.shouldRefreshPlayback()) {
                refreshInternal(guildId = guildId)
            }
        }
    }

    private suspend fun observeConnection() {
        observeRealtimeConnectionStateUseCase().collect { connectionState ->
            _playbackMonitorState.update { currentPlaybackMonitorState ->
                currentPlaybackMonitorState.copy(isConnected = connectionState == RealtimeConnectionState.Connected)
            }
        }
    }

    private suspend fun pollFallback(guildId: String) {
        while (currentCoroutineContext().isActive) {
            delay(duration = 15_000.milliseconds)
            refreshInternal(guildId = guildId)
        }
    }

    private suspend fun refreshInternal(guildId: String) {
        getCurrentTrackUseCase(guildId = guildId).collect { result ->
            when (result) {
                Resource.Loading -> _playbackMonitorState.update { currentPlaybackMonitorState -> currentPlaybackMonitorState.copy(
                    isRefreshing = true
                ) }

                is Resource.Success -> _playbackMonitorState.update { currentPlaybackMonitorState ->
                    currentPlaybackMonitorState.copy(
                        guildId = guildId,
                        currentTrack = result.data,
                        isRefreshing = false,
                        lastUpdatedAtMillis = Clock.System.now()
                            .toEpochMilliseconds(),
                        errorMessage = null
                    )
                }

                is Resource.Error -> _playbackMonitorState.update { currentPlaybackMonitorState ->
                    currentPlaybackMonitorState.copy(
                        isRefreshing = false,
                        errorMessage = result.error.message
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
            MobileRealtimeEventNames.BotVoiceUserCountChanged
        )
    }
}
