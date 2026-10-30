package com.dc.melodiasmario.core.network.realtime

import com.dc.melodiasmario.core.model.realtime.RealtimeConnectionState
import com.dc.melodiasmario.core.model.realtime.RealtimeEvent
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.StateFlow

/**
 * Provides mobile realtime client for the realtime layer.
 */
expect class MobileRealtimeClient() {
    /**
     * Connection state exposed by mobile realtime client.
     */
    val connectionState: StateFlow<RealtimeConnectionState>

    /**
     * Observes realtime events for the requested guild.
     */
    fun observeGuild(
        accessToken: String,
        guildId: String
    ): Flow<RealtimeEvent>

    /**
     * Disconnects the realtime transport and releases its active subscription.
     */
    suspend fun disconnect(): Unit
}
