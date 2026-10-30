package com.dc.melodiasmario.core.domain.playlist

import com.dc.melodiasmario.core.model.playlist.Playlist
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand

interface PlaylistsRepository {
    suspend fun setArtwork(playlistId: String, artworkUri: String?)
    suspend fun getPlaylists(guildId: String): List<Playlist>
    suspend fun deletePlaylist(playlistId: String)
    suspend fun renamePlaylist(playlistId: String, newName: String)
    suspend fun createPlaylist(guildId: String, playlistName: String): Playlist
    suspend fun addTrack(playlistId: String, songUrl: String): BotControlCommand
    suspend fun createPlaylistFromTracks(
        guildId: String,
        playlistName: String,
        trackUrls: List<String>,
    ): Playlist

}
