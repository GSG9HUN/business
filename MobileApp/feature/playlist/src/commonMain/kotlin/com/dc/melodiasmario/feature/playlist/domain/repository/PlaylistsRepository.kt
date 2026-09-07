package com.dc.melodiasmario.feature.playlist.domain.repository

import com.dc.melodiasmario.feature.playlist.domain.model.Playlist

interface PlaylistsRepository {
    suspend fun getPlaylists(guildId: String): List<Playlist>
    suspend fun deletePlaylist(playlistId: String)
    suspend fun renamePlaylist(playlistId: String, newName: String)
    suspend fun createPlaylist(guildId: String, playlistName: String)
}