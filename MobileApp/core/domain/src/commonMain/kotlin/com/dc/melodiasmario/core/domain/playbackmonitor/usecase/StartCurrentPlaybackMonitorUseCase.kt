package com.dc.melodiasmario.core.domain.playbackmonitor.usecase

import com.dc.melodiasmario.core.domain.playbackmonitor.CurrentPlaybackMonitorRepository
import org.koin.core.annotation.Single

@Single
class StartCurrentPlaybackMonitorUseCase(
    private val repository: CurrentPlaybackMonitorRepository,
) {
    operator fun invoke(guildId: String) = repository.start(guildId)
}