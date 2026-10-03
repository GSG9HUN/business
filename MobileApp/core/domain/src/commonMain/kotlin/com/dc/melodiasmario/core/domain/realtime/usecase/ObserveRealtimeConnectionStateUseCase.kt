package com.dc.melodiasmario.core.domain.realtime.usecase

import com.dc.melodiasmario.core.domain.realtime.RealtimeRepository
import com.dc.melodiasmario.core.model.realtime.RealtimeConnectionState
import kotlinx.coroutines.flow.StateFlow
import org.koin.core.annotation.Single

@Single
class ObserveRealtimeConnectionStateUseCase(
    private val realtimeRepository: RealtimeRepository
) {
    operator fun invoke(): StateFlow<RealtimeConnectionState> {
        return realtimeRepository.connectionState
    }
}