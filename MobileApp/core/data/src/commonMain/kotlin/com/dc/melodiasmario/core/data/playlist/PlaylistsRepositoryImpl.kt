package com.dc.melodiasmario.core.data.playlist

import com.dc.melodiasmario.core.data.auth.AuthorizedSessionProvider
import com.dc.melodiasmario.core.domain.playlist.PlaylistsRepository
import com.dc.melodiasmario.core.model.playlist.Playlist
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.network.playlist.PlaylistsRemoteDataSource
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

    override suspend fun createPlaylist(guildId: String, playlistName: String): Playlist {
        val accessToken = authorizedSessionProvider.getValidSession()
        return playlistsRemoteDataSource.createPlaylist(
            accessToken = accessToken,
            guildId = guildId,
            playlistName = playlistName
        ).toDomain()
    }

    override suspend fun addTrack(playlistId: String, songUrl: String): BotControlCommand {
        return playlistsRemoteDataSource.addTrack(authorizedSessionProvider.getValidSession(), playlistId, songUrl)
    }
    override suspend fun createPlaylistFromTracks(
        guildId: String,
        playlistName: String,
        trackUrls: List<String>,
    ): Playlist {
        val accessToken = authorizedSessionProvider.getValidSession()

        return playlistsRemoteDataSource.createPlaylistFromTracks(
            accessToken = accessToken,
            guildId = guildId,
            playlistName = playlistName,
            trackUrls = trackUrls,
        ).toDomain()
    }

}
