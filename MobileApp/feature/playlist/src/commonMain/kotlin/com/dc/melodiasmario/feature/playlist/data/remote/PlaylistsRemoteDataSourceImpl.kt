package com.dc.melodiasmario.feature.playlist.data.remote

import com.dc.melodiasmario.feature.playlist.data.remote.dto.PlaylistDto
import org.koin.core.annotation.Single

@Single(binds = [PlaylistsRemoteDataSource::class])
class PlaylistsRemoteDataSourceImpl(
    private val playlistsApiService: PlaylistsApiService
) : PlaylistsRemoteDataSource {

    override suspend fun getPlaylists(accessToken: String, guildId: String): List<PlaylistDto> {
        return playlistsApiService.getPlaylists(accessToken = accessToken, guildId = guildId)
    }

    override suspend fun deletePlaylist(accessToken: String, playlistId: String) {
        playlistsApiService.deletePlaylist(accessToken = accessToken, playlistId = playlistId)
    }

    override suspend fun renamePlaylist(
        accessToken: String,
        playlistId: String,
        newName: String
    ) {
        playlistsApiService.renamePlaylists(
            accessToken = accessToken,
            playlistId = playlistId,
            newName = newName
        )
    }

    override suspend fun createPlaylist(
        accessToken: String,
        guildId: String,
        playlistName: String
    ) {
        playlistsApiService.createPlaylist(
            accessToken = accessToken,
            guildId = guildId,
            playlistName = playlistName
        )
    }
}
