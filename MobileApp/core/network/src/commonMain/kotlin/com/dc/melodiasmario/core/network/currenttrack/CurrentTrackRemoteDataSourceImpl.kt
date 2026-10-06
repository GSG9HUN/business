package com.dc.melodiasmario.core.network.currenttrack

import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.model.currenttrack.PlaybackStatus
import org.koin.core.annotation.Single

@Single(binds = [CurrentTrackRemoteDataSource::class])
class CurrentTrackRemoteDataSourceImpl(
    private val currentTrackApiService: CurrentTrackApiService
) : CurrentTrackRemoteDataSource {
    override suspend fun getPlaybackStatus(
        accessToken: String,
        guildId: String
    ): PlaybackStatus {
        return currentTrackApiService.getPlaybackStatus(accessToken = accessToken, guildId = guildId)
            .toDomain()
    }

    override suspend fun nextTrack(accessToken: String, guildId: String): BotControlCommand {
        return currentTrackApiService.nextTrack(accessToken = accessToken, guildId = guildId).toDomain()
    }

    override suspend fun previousTrack(accessToken: String, guildId: String): BotControlCommand {
        return currentTrackApiService.previousTrack(accessToken = accessToken, guildId = guildId).toDomain()
    }

    override suspend fun play(accessToken: String, guildId: String): BotControlCommand {
        return currentTrackApiService.play(accessToken = accessToken, guildId = guildId).toDomain()
    }

    override suspend fun pause(accessToken: String, guildId: String): BotControlCommand {
        return currentTrackApiService.pause(accessToken = accessToken, guildId = guildId).toDomain()
    }

    override suspend fun setRepeatMode(accessToken: String, guildId: String, mode: String) {
        return currentTrackApiService.setRepeatMode(
            accessToken = accessToken,
            guildId = guildId,
            mode = mode,
        )
    }
}
