package com.dc.melodiasmario.core.domain.playbackmonitor.usecase

import com.dc.melodiasmario.core.domain.playbackmonitor.CurrentPlaybackMonitorRepository
import com.dc.melodiasmario.core.model.playbackmonitor.CurrentPlaybackMonitorState
import kotlinx.coroutines.flow.StateFlow
import org.koin.core.annotation.Factory

/**
 * Exposes observe current playback monitor as a domain operation.
 */
@Factory
class ObserveCurrentPlaybackMonitorUseCase(
    private val currentPlaybackMonitorRepository: CurrentPlaybackMonitorRepository
) {
    /**
     * Executes the observe current playback monitor domain operation.
     */
    operator fun invoke(): StateFlow<CurrentPlaybackMonitorState> = currentPlaybackMonitorRepository.playbackMonitorState
}
