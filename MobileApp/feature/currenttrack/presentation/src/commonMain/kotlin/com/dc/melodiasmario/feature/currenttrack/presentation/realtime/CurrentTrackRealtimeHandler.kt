package com.dc.melodiasmario.feature.currenttrack.presentation.realtime

import com.dc.melodiasmario.core.domain.realtime.usecase.ObserveGuildRealtimeUseCase
import com.dc.melodiasmario.core.domain.realtime.usecase.ObserveRealtimeConnectionStateUseCase
import com.dc.melodiasmario.core.model.realtime.RealtimeConnectionState
import com.dc.melodiasmario.core.model.realtime.RealtimeEvent
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Job
import kotlinx.coroutines.launch

internal class CurrentTrackRealtimeHandler(
    private val scope: CoroutineScope,
    private val observeGuildRealtimeUseCase: ObserveGuildRealtimeUseCase,
    private val observeRealtimeConnectionStateUseCase: ObserveRealtimeConnectionStateUseCase,
    private val pollingController: CurrentTrackPollingController,
    private val onRealtimeEvent: suspend (RealtimeEvent) -> Unit,
) {
    private var realtimeJob: Job? = null
    private var connectionStateJob: Job? = null

    fun start(guildId: String) {
        startRealtime(guildId)
        observeConnectionState()
    }

    fun clear() {
        realtimeJob?.cancel()
        realtimeJob = null
        connectionStateJob?.cancel()
        connectionStateJob = null
        pollingController.clear()
    }

    private fun startRealtime(guildId: String) {
        realtimeJob?.cancel()
        realtimeJob = scope.launch {
            observeGuildRealtimeUseCase(guildId).collect { event ->
                onRealtimeEvent(event)
            }
        }
    }

    private fun observeConnectionState() {
        connectionStateJob?.cancel()
        connectionStateJob = scope.launch {
            observeRealtimeConnectionStateUseCase().collect { state ->
                when (state) {
                    RealtimeConnectionState.Connected -> pollingController.stop()
                    RealtimeConnectionState.Disconnected,
                    RealtimeConnectionState.Reconnecting -> pollingController.start()
                    RealtimeConnectionState.Connecting -> Unit
                }
            }
        }
    }
}
