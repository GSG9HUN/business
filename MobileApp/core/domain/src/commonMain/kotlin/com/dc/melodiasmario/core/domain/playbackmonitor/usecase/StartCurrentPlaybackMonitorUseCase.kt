package com.dc.melodiasmario.core.domain.playbackmonitor.usecase

import com.dc.melodiasmario.core.domain.playbackmonitor.CurrentPlaybackMonitorRepository
import org.koin.core.annotation.Factory

/**
 * Exposes start current playback monitor as a domain operation.
 */
@Factory
class StartCurrentPlaybackMonitorUseCase(
    private val currentPlaybackMonitorRepository: CurrentPlaybackMonitorRepository
) {
    /**
     * Executes the start current playback monitor domain operation.
     */
    operator fun invoke(guildId: String): Unit = currentPlaybackMonitorRepository.start(guildId = guildId)
}
