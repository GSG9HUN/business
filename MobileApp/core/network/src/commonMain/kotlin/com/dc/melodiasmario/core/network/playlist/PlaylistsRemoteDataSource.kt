package com.dc.melodiasmario.core.network.playlist

import com.dc.melodiasmario.core.network.playlist.dto.PlaylistDto

interface PlaylistsRemoteDataSource {
    suspend fun getPlaylists(accessToken: String, guildId: String): List<PlaylistDto>
    suspend fun deletePlaylist(accessToken: String, playlistId: String)
    suspend fun renamePlaylist(
        accessToken: String,
        playlistId: String,
        newName: String
    )
    suspend fun createPlaylist(
        accessToken: String,
        guildId: String,
        playlistName: String
    )
}