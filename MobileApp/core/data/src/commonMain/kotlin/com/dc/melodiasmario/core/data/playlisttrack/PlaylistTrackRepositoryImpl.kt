package com.dc.melodiasmario.core.data.playlisttrack

import com.dc.melodiasmario.core.data.auth.AuthorizedSessionProvider
import com.dc.melodiasmario.core.domain.playlisttrack.PlaylistTrackRepository
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.model.playlist.PlaylistDetail
import com.dc.melodiasmario.core.network.playlist.PlaylistsRemoteDataSource
import com.dc.melodiasmario.core.network.playlisttrack.PlaylistTrackRemoteDataSource
import org.koin.core.annotation.Single

/**
 * Implements playlist track data access using the injected data sources.
 */
@Single(binds = [PlaylistTrackRepository::class])
class PlaylistTrackRepositoryImpl(
    private val playlistTrackRemoteDataSource: PlaylistTrackRemoteDataSource,
    private val playlistsRemoteDataSource: PlaylistsRemoteDataSource,
    private val authorizedSessionProvider: AuthorizedSessionProvider
) : PlaylistTrackRepository {
    override suspend fun getPlaylist(playlistId: String): PlaylistDetail {
        val accessToken = authorizedSessionProvider.getValidSession()
        return playlistTrackRemoteDataSource.getPlaylist(
            accessToken = accessToken,
            playlistId = playlistId
        )
    }

    override suspend fun removeTrack(playlistId: String, trackNumber: Int): Unit {
        val accessToken = authorizedSessionProvider.getValidSession()
        playlistTrackRemoteDataSource.removeTrack(
            accessToken = accessToken,
            playlistId = playlistId,
            trackNumber = trackNumber
        )
    }

    override suspend fun addTrack(playlistId: String, query: String): BotControlCommand {
        val accessToken = authorizedSessionProvider.getValidSession()
        return playlistsRemoteDataSource.addTrack(
            accessToken = accessToken,
            playlistId = playlistId,
            songUrl = query
        )
    }

    override suspend fun loadPlaylist(playlistId: String): BotControlCommand {
        val accessToken = authorizedSessionProvider.getValidSession()
        return playlistTrackRemoteDataSource.loadPlaylist(
            accessToken = accessToken,
            playlistId = playlistId
        )
    }
}
