package com.dc.melodiasmario.core.domain.playbackmonitor.usecase

import com.dc.melodiasmario.core.domain.playbackmonitor.CurrentPlaybackMonitorRepository
import org.koin.core.annotation.Factory

@Factory
class ObserveCurrentPlaybackMonitorUseCase(
    private val currentPlaybackMonitorRepository: CurrentPlaybackMonitorRepository
) {
    operator fun invoke() = currentPlaybackMonitorRepository.state
}