package com.dc.melodiasmario.core.data.playlist

import com.dc.melodiasmario.core.data.auth.AuthorizedSessionProvider
import com.dc.melodiasmario.core.domain.playlist.PlaylistsRepository
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.model.playlist.Playlist
import com.dc.melodiasmario.core.network.playlist.PlaylistsRemoteDataSource
import org.koin.core.annotation.Single

/**
 * Implements playlists data access using the injected data sources.
 */
@Single(binds = [PlaylistsRepository::class])
class PlaylistsRepositoryImpl(
    private val playlistsRemoteDataSource: PlaylistsRemoteDataSource,
    private val authorizedSessionProvider: AuthorizedSessionProvider
) : PlaylistsRepository {
    override suspend fun setArtwork(playlistId: String, artworkUri: String?): Unit {
        val accessToken = authorizedSessionProvider.getValidSession()
        playlistsRemoteDataSource.setArtwork(
            accessToken = accessToken,
            playlistId = playlistId,
            artworkUri = artworkUri
        )
    }

    override suspend fun getPlaylists(guildId: String): List<Playlist> {
        val accessToken = authorizedSessionProvider.getValidSession()
        return playlistsRemoteDataSource.getPlaylists(
            accessToken = accessToken,
            guildId = guildId
        )
    }

    override suspend fun deletePlaylist(playlistId: String): Unit {
        val accessToken = authorizedSessionProvider.getValidSession()
        playlistsRemoteDataSource.deletePlaylist(
            accessToken = accessToken,
            playlistId = playlistId
        )
    }

    override suspend fun renamePlaylist(playlistId: String, newName: String): Unit {
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
        )
    }

    override suspend fun addTrack(playlistId: String, songUrl: String): BotControlCommand {
        val accessToken = authorizedSessionProvider.getValidSession()
        return playlistsRemoteDataSource.addTrack(
            accessToken = accessToken,
            playlistId = playlistId,
            songUrl = songUrl
        )
    }

    override suspend fun createPlaylistFromTracks(
        guildId: String,
        playlistName: String,
        trackUrls: List<String>
    ): Playlist {
        val accessToken = authorizedSessionProvider.getValidSession()

        return playlistsRemoteDataSource.createPlaylistFromTracks(
            accessToken = accessToken,
            guildId = guildId,
            playlistName = playlistName,
            trackUrls = trackUrls
        )
    }
}
