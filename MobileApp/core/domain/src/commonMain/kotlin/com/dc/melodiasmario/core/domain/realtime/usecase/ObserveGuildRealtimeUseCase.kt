package com.dc.melodiasmario.core.domain.realtime.usecase

import com.dc.melodiasmario.core.domain.realtime.RealtimeRepository
import com.dc.melodiasmario.core.model.realtime.RealtimeEvent
import kotlinx.coroutines.flow.Flow
import org.koin.core.annotation.Factory

/**
 * Exposes observe guild realtime as a domain operation.
 */
@Factory
class ObserveGuildRealtimeUseCase(
    private val realtimeRepository: RealtimeRepository
) {
    /**
     * Executes the observe guild realtime domain operation.
     */
    operator fun invoke(guildId: String): Flow<RealtimeEvent> {
        return realtimeRepository.observeGuildRealtime(guildId = guildId)
    }
}
