package com.dc.melodiasmario.core.domain.playbackmonitor.usecase

import com.dc.melodiasmario.core.domain.playbackmonitor.CurrentPlaybackMonitorRepository
import org.koin.core.annotation.Factory

@Factory
class RefreshCurrentPlaybackMonitorUseCase(
    private val currentPlaybackMonitorRepository: CurrentPlaybackMonitorRepository
) {
    operator fun invoke() = currentPlaybackMonitorRepository.refresh()
}