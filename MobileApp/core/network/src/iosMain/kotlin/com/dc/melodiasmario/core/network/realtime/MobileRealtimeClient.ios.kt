package com.dc.melodiasmario.core.network.realtime

import com.dc.melodiasmario.core.model.realtime.RealtimeConnectionState
import com.dc.melodiasmario.core.model.realtime.RealtimeEvent
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.emptyFlow

actual class MobileRealtimeClient {
    private val _connectionState = MutableStateFlow(RealtimeConnectionState.Disconnected)

    actual val connectionState: StateFlow<RealtimeConnectionState> = _connectionState

    actual fun observeGuild(
        accessToken: String,
        guildId: String
    ): Flow<RealtimeEvent> = emptyFlow()

    actual suspend fun connect(accessToken: String, guildId: String) = Unit

    actual suspend fun disconnect() {
        _connectionState.value = RealtimeConnectionState.Disconnected
    }
}
