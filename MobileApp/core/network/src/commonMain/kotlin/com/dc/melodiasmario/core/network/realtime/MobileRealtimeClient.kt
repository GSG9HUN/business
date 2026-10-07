package com.dc.melodiasmario.core.network.realtime

import com.dc.melodiasmario.core.model.realtime.RealtimeConnectionState
import com.dc.melodiasmario.core.model.realtime.RealtimeEvent
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.StateFlow

expect class MobileRealtimeClient() {
    val connectionState: StateFlow<RealtimeConnectionState>

    fun observeGuild(
        accessToken: String,
        guildId: String
    ): Flow<RealtimeEvent>

    suspend fun connect(
        accessToken: String,
        guildId: String
    )

    suspend fun disconnect()
}
