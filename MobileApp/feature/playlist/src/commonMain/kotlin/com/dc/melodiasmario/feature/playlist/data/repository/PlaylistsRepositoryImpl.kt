package com.dc.melodiasmario.feature.playlist.data.repository

import com.dc.melodiasmario.core.auth.data.session.AuthorizedSessionProvider
import com.dc.melodiasmario.feature.playlist.data.remote.PlaylistsRemoteDataSource
import com.dc.melodiasmario.feature.playlist.domain.model.Playlist
import com.dc.melodiasmario.feature.playlist.domain.repository.PlaylistsRepository
import org.koin.core.annotation.Single

@Single(binds = [PlaylistsRepository::class])
class PlaylistsRepositoryImpl(
    private val playlistsRemoteDataSource: PlaylistsRemoteDataSource,
    private val authorizedSessionProvider: AuthorizedSessionProvider
) : PlaylistsRepository {
    override suspend fun getPlaylists(guildId: String): List<Playlist> {
        val accessToken = authorizedSessionProvider.getValidSession()
        return playlistsRemoteDataSource.getPlaylists(
            accessToken = accessToken, guildId = guildId
        ).map { it.toDomain() }
    }

    override suspend fun deletePlaylist(playlistId: String) {
        val accessToken = authorizedSessionProvider.getValidSession()
        playlistsRemoteDataSource.deletePlaylist(
            accessToken = accessToken,
            playlistId = playlistId
        )
    }

    override suspend fun renamePlaylist(playlistId: String, newName: String) {
        val accessToken = authorizedSessionProvider.getValidSession()
        playlistsRemoteDataSource.renamePlaylist(
            accessToken = accessToken,
            playlistId = playlistId,
            newName = newName
        )
    }

    override suspend fun createPlaylist(guildId: String, playlistName: String) {
        val accessToken = authorizedSessionProvider.getValidSession()
        playlistsRemoteDataSource.createPlaylist(
            accessToken = accessToken,
            guildId = guildId,
            playlistName = playlistName
        )
    }
}