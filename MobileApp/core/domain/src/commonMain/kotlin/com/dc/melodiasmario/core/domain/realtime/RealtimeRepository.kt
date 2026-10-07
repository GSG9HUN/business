package com.dc.melodiasmario.core.domain.realtime

import com.dc.melodiasmario.core.model.realtime.RealtimeConnectionState
import com.dc.melodiasmario.core.model.realtime.RealtimeEvent
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.StateFlow

interface RealtimeRepository {
    val connectionState: StateFlow<RealtimeConnectionState>

    fun observeGuildRealtime(guildId: String): Flow<RealtimeEvent>

    suspend fun disconnect()
}