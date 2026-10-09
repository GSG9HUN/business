package com.dc.melodiasmario.core.data.playbackcommand

import com.dc.melodiasmario.core.domain.currenttrack.CurrentTrackRepository
import com.dc.melodiasmario.core.domain.playbackcommand.PlaybackCommandRepository
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import org.koin.core.annotation.Single

@Single(binds = [PlaybackCommandRepository::class])
class PlaybackCommandRepositoryImpl(
    private val currentTrackRepository: CurrentTrackRepository,
) : PlaybackCommandRepository {
    override suspend fun previous(guildId: String): BotControlCommand {
        return currentTrackRepository.previousTrack(guildId)
    }

    override suspend fun next(guildId: String): BotControlCommand {
        return currentTrackRepository.nextTrack(guildId)
    }

    override suspend fun playPause(
        guildId: String,
        isPlaying: Boolean,
    ): BotControlCommand {
        return if (isPlaying) {
            currentTrackRepository.pause(guildId)
        } else {
            currentTrackRepository.play(guildId)
        }
    }
}