package com.dc.melodiasmario.core.data.currenttrack

import com.dc.melodiasmario.core.data.auth.AuthorizedSessionProvider
import com.dc.melodiasmario.core.domain.currenttrack.CurrentTrackRepository
import com.dc.melodiasmario.core.model.currenttrack.PlaybackStatus
import com.dc.melodiasmario.core.model.currenttrack.RepeatMode
import com.dc.melodiasmario.core.network.currenttrack.CurrentTrackRemoteDataSource
import org.koin.core.annotation.Single

@Single(binds = [CurrentTrackRepository::class])
class CurrentTrackRepositoryImpl(
    private val currentTrackRemoteDataSource: CurrentTrackRemoteDataSource,
    private val authorizedSessionProvider: AuthorizedSessionProvider
) : CurrentTrackRepository {
    override suspend fun getPlaybackStatus(guildId: String): PlaybackStatus {
        val accessToken = authorizedSessionProvider.getValidSession()
        return currentTrackRemoteDataSource.getPlaybackStatus(
            accessToken = accessToken,
            guildId = guildId,
        )
    }

    override suspend fun nextTrack(guildId: String) {
        val accessToken = authorizedSessionProvider.getValidSession()
        currentTrackRemoteDataSource.nextTrack(
            accessToken = accessToken,
            guildId = guildId,
        )
    }

    override suspend fun previousTrack(guildId: String) {
        val accessToken = authorizedSessionProvider.getValidSession()
        currentTrackRemoteDataSource.previousTrack(
            accessToken = accessToken,
            guildId = guildId,
        )
    }

    override suspend fun play(guildId: String) {
        val accessToken = authorizedSessionProvider.getValidSession()
        currentTrackRemoteDataSource.play(accessToken = accessToken, guildId = guildId)
    }

    override suspend fun pause(guildId: String) {
        val accessToken = authorizedSessionProvider.getValidSession()
        currentTrackRemoteDataSource.pause(accessToken = accessToken, guildId = guildId)
    }

    override suspend fun setRepeatMode(guildId: String, repeatMode: RepeatMode) {
        val accessToken = authorizedSessionProvider.getValidSession()
        currentTrackRemoteDataSource.setRepeatMode(
            accessToken = accessToken,
            guildId = guildId,
            mode = repeatMode.toApiValue(),
        )
    }

    private fun RepeatMode.toApiValue() = when (this) {
        RepeatMode.NONE -> "none"
        RepeatMode.ONE -> "one"
        RepeatMode.ALL -> "all"
    }
}
