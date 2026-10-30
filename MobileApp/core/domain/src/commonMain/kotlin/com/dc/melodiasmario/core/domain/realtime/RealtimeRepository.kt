package com.dc.melodiasmario.core.domain.realtime

import com.dc.melodiasmario.core.model.realtime.RealtimeConnectionState
import com.dc.melodiasmario.core.model.realtime.RealtimeEvent
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.StateFlow

/**
 * Defines realtime data operations independently of their concrete data sources.
 */
interface RealtimeRepository {
    /**
     * Connection state exposed by realtime repository.
     */
    val connectionState: StateFlow<RealtimeConnectionState>

    /**
     * Exposes the realtime event stream for the requested guild.
     */
    fun observeGuildRealtime(guildId: String): Flow<RealtimeEvent>

    /**
     * Disconnects the realtime transport and releases its active subscription.
     */
    suspend fun disconnect(): Unit
}
