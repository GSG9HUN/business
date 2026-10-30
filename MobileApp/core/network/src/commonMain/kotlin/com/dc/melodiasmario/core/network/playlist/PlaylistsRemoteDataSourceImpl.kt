package com.dc.melodiasmario.core.network.playlist

import com.dc.melodiasmario.core.network.playlist.dto.CreatePlaylistFromTracksRequestDto
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.model.playlist.Playlist
import org.koin.core.annotation.Single

@Single(binds = [PlaylistsRemoteDataSource::class])
class PlaylistsRemoteDataSourceImpl(
    private val playlistsApiService: PlaylistsApiService
) : PlaylistsRemoteDataSource {

    override suspend fun setArtwork(accessToken: String, playlistId: String, artworkUri: String?) {
        playlistsApiService.setArtwork(
            accessToken = accessToken,
            playlistId = playlistId,
            artworkUri = artworkUri
        )
    }

    override suspend fun getPlaylists(accessToken: String, guildId: String): List<Playlist> {
        return playlistsApiService.getPlaylists(accessToken = accessToken, guildId = guildId)
            .map { it.toDomain() }
    }

    override suspend fun deletePlaylist(accessToken: String, playlistId: String) {
        playlistsApiService.deletePlaylist(accessToken = accessToken, playlistId = playlistId)
    }

    override suspend fun renamePlaylist(
        accessToken: String, playlistId: String, newName: String
    ) {
        playlistsApiService.renamePlaylist(
            accessToken = accessToken, playlistId = playlistId, newName = newName
        )
    }

    override suspend fun createPlaylist(
        accessToken: String, guildId: String, playlistName: String
    ): Playlist {
        return playlistsApiService.createPlaylist(
            accessToken = accessToken, guildId = guildId, playlistName = playlistName
        ).toDomain()
    }

    override suspend fun addTrack(
        accessToken: String, playlistId: String, songUrl: String
    ): BotControlCommand {
        return playlistsApiService.addTrack(
            accessToken = accessToken,
            playlistId = playlistId,
            songUrl = songUrl
        ).toDomain()
    }

    override suspend fun createPlaylistFromTracks(
        accessToken: String,
        guildId: String,
        playlistName: String,
        trackUrls: List<String>,
    ): Playlist {
        return playlistsApiService.createPlaylistFromTracks(
            accessToken = accessToken,
            guildId = guildId,
            request = CreatePlaylistFromTracksRequestDto(
                playlistName = playlistName,
                trackUrls = trackUrls,
            ),
        ).toDomain()
    }

}
