package com.dc.melodiasmario.core.network.playlisttrack

import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.model.playlist.PlaylistDetail
import org.koin.core.annotation.Single

/**
 * Adapts remote playlist track responses to domain-facing data.
 */
@Single(binds = [PlaylistTrackRemoteDataSource::class])
class PlaylistTrackRemoteDataSourceImpl(
    private val playlistTrackApiService: PlaylistTrackApiService
) : PlaylistTrackRemoteDataSource {
    override suspend fun getPlaylist(accessToken: String, playlistId: String): PlaylistDetail {
        return playlistTrackApiService.getPlaylist(
            accessToken = accessToken,
            playlistId = playlistId
        )
            .toDomain()
    }

    override suspend fun removeTrack(accessToken: String, playlistId: String, trackNumber: Int): Unit {
        return playlistTrackApiService.removeTrack(
            accessToken = accessToken,
            playlistId = playlistId,
            trackNumber = trackNumber
        )
    }

    override suspend fun loadPlaylist(accessToken: String, playlistId: String): BotControlCommand {
        return playlistTrackApiService.loadPlaylist(
            accessToken = accessToken,
            playlistId = playlistId
        )
            .toDomain()
    }
}
