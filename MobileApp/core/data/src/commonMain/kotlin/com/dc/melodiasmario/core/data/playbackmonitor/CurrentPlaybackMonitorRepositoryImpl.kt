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
    private var refreshJob: Job? = null
    private var pollingJob: Job? = null
    private var currentGuildId: String? = null

    override fun start(guildId: String) {
        if (currentGuildId == guildId && monitorJob?.isActive == true) return

        monitorJob?.cancel()
        refreshJob?.cancel()
        pollingJob?.cancel()

        currentGuildId = guildId
        _state.value = CurrentPlaybackMonitorState(
            guildId = guildId,
            isRefreshing = true,
        )
        monitorJob = scope.launch {
            launch { observeRealtime(guildId) }
            launch { observeConnection(guildId, this) }
            refreshInternal(guildId)
        }
    }

    override fun stop() {
        monitorJob?.cancel()
        refreshJob?.cancel()
        pollingJob?.cancel()

        monitorJob = null
        refreshJob = null
        pollingJob = null
        currentGuildId = null
        _state.value = CurrentPlaybackMonitorState()
    }

    override fun refresh() {
        val guildId = currentGuildId ?: return

        refreshJob?.cancel()
        refreshJob = scope.launch {
            refreshInternal(guildId)
        }
    }

    private suspend fun observeRealtime(guildId: String) {
        observeGuildRealtimeUseCase(guildId).collect { event ->
            if (event.eventName.shouldRefreshPlayback()) {
                refreshInternal(guildId)
            }
        }
    }

    private suspend fun observeConnection(
        guildId: String,
        monitorScope: CoroutineScope,
    ) {
        observeRealtimeConnectionStateUseCase().collect { connectionState ->
            if (!isActiveGuild(guildId)) return@collect

            _state.update {
                if (!isActiveGuild(guildId)) {
                    it
                } else {
                    it.copy(isConnected = connectionState == RealtimeConnectionState.Connected)
                }
            }

            if (connectionState.shouldPollFallback()) {
                if (pollingJob?.isActive != true) {
                    pollingJob = monitorScope.launch {
                        pollFallback(guildId)
                    }
                }
            } else {
                pollingJob?.cancel()
                pollingJob = null
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
            if (!isActiveGuild(guildId)) return@collect

            when (result) {
                Resource.Loading -> _state.update {
                    if (!isActiveGuild(guildId)) {
                        it
                    } else {
                        it.copy(
                            isRefreshing = true
                        )
                    }
                }

                is Resource.Success -> _state.update {
                    if (!isActiveGuild(guildId)) {
                        it
                    } else {
                        it.copy(
                            guildId = guildId,
                            currentTrack = result.data,
                            isRefreshing = false,
                            lastUpdatedAtMillis = Clock.System.now().toEpochMilliseconds(),
                            errorMessage = null,
                        )
                    }
                }

                is Resource.Error -> _state.update {
                    if (!isActiveGuild(guildId)) {
                        it
                    } else {
                        it.copy(
                            isRefreshing = false,
                            errorMessage = result.error.message,
                        )
                    }
                }
            }
        }
    }

    private fun isActiveGuild(guildId: String): Boolean {
        return currentGuildId == guildId
    }

    private fun RealtimeConnectionState.shouldPollFallback(): Boolean {
        return this == RealtimeConnectionState.Disconnected ||
            this == RealtimeConnectionState.Reconnecting
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
