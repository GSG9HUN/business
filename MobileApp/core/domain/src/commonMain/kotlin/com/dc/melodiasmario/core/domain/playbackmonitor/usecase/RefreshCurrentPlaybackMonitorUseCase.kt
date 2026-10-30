package com.dc.melodiasmario.core.domain.playbackmonitor.usecase

import com.dc.melodiasmario.core.domain.playbackmonitor.CurrentPlaybackMonitorRepository
import org.koin.core.annotation.Factory

/**
 * Exposes refresh current playback monitor as a domain operation.
 */
@Factory
class RefreshCurrentPlaybackMonitorUseCase(
    private val currentPlaybackMonitorRepository: CurrentPlaybackMonitorRepository
) {
    /**
     * Executes the refresh current playback monitor domain operation.
     */
    operator fun invoke(): Unit = currentPlaybackMonitorRepository.refresh()
}
