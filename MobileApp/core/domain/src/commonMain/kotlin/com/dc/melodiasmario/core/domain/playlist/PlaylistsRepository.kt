package com.dc.melodiasmario.core.domain.playlist

import com.dc.melodiasmario.core.model.playlist.Playlist

interface PlaylistsRepository {
    suspend fun getPlaylists(guildId: String): List<Playlist>
    suspend fun deletePlaylist(playlistId: String)
    suspend fun renamePlaylist(playlistId: String, newName: String)
    suspend fun createPlaylist(guildId: String, playlistName: String)
}