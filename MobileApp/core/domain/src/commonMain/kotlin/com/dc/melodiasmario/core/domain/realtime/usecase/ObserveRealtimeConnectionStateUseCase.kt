package com.dc.melodiasmario.core.domain.realtime.usecase

import com.dc.melodiasmario.core.domain.realtime.RealtimeRepository
import com.dc.melodiasmario.core.model.realtime.RealtimeConnectionState
import kotlinx.coroutines.flow.StateFlow
import org.koin.core.annotation.Factory

/**
 * Exposes observe realtime connection state as a domain operation.
 */
@Factory
class ObserveRealtimeConnectionStateUseCase(
    private val realtimeRepository: RealtimeRepository
) {
    /**
     * Executes the observe realtime connection state domain operation.
     */
    operator fun invoke(): StateFlow<RealtimeConnectionState> {
        return realtimeRepository.connectionState
    }
}
