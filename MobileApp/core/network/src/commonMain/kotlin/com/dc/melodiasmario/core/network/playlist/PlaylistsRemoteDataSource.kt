package com.dc.melodiasmario.core.network.playlist

import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.model.playlist.Playlist

interface PlaylistsRemoteDataSource {
    suspend fun setArtwork(accessToken: String, playlistId: String, artworkUri: String?)
    suspend fun getPlaylists(accessToken: String, guildId: String): List<Playlist>
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
    ): Playlist

    suspend fun addTrack(
        accessToken: String,
        playlistId: String,
        songUrl: String
    ): BotControlCommand

    suspend fun createPlaylistFromTracks(
        accessToken: String,
        guildId: String,
        playlistName: String,
        trackUrls: List<String>,
    ): Playlist

}
