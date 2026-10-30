package com.dc.melodiasmario.core.network.realtime

import com.dc.melodiasmario.core.model.realtime.RealtimeConnectionState
import com.dc.melodiasmario.core.model.realtime.RealtimeEvent
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.emptyFlow

/**
 * Provides mobile realtime client for the realtime layer.
 */
actual class MobileRealtimeClient {
    private val _connectionState = MutableStateFlow(value = RealtimeConnectionState.Disconnected)

    /**
     * Connection state exposed by mobile realtime client.
     */
    actual val connectionState: StateFlow<RealtimeConnectionState> = _connectionState

    /**
     * Observes realtime events for the requested guild.
     */
    actual fun observeGuild(
        accessToken: String,
        guildId: String
    ): Flow<RealtimeEvent> = emptyFlow()

    /**
     * Disconnects the realtime transport and releases its active subscription.
     */
    actual suspend fun disconnect(): Unit {
        _connectionState.value = RealtimeConnectionState.Disconnected
    }
}
