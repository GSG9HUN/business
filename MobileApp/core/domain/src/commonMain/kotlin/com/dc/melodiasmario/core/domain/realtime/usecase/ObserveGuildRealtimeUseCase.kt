package com.dc.melodiasmario.core.domain.realtime.usecase

import com.dc.melodiasmario.core.domain.realtime.RealtimeRepository
import com.dc.melodiasmario.core.model.realtime.RealtimeEvent
import kotlinx.coroutines.flow.Flow
import org.koin.core.annotation.Single

@Single
class ObserveGuildRealtimeUseCase(
    private val realtimeRepository: RealtimeRepository
) {
    operator fun invoke(guildId: String): Flow<RealtimeEvent> {
        return realtimeRepository.observeGuildRealtime(guildId)
    }
}