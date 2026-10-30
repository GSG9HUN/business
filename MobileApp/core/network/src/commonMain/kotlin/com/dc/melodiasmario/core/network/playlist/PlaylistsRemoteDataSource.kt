package com.dc.melodiasmario.core.network.playlist

import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.model.playlist.Playlist

/**
 * Defines remote playlists data operations.
 */
interface PlaylistsRemoteDataSource {
    /**
     * Sets the playlist cover URL, or restores automatic artwork when the value is absent.
     */
    suspend fun setArtwork(accessToken: String, playlistId: String, artworkUri: String?): Unit

    /**
     * Retrieves the playlists belonging to the requested guild.
     */
    suspend fun getPlaylists(accessToken: String, guildId: String): List<Playlist>

    /**
     * Deletes the identified saved playlist.
     */
    suspend fun deletePlaylist(accessToken: String, playlistId: String): Unit

    /**
     * Changes the name of the identified playlist.
     */
    suspend fun renamePlaylist(
        accessToken: String,
        playlistId: String,
        newName: String
    ): Unit

    /**
     * Creates a named playlist for the requested guild.
     */
    suspend fun createPlaylist(
        accessToken: String,
        guildId: String,
        playlistName: String
    ): Playlist

    /**
     * Requests adding the supplied music query or URL to a saved playlist.
     */
    suspend fun addTrack(
        accessToken: String,
        playlistId: String,
        songUrl: String
    ): BotControlCommand

    /**
     * Defines the bulk playlist-creation operation for an ordered track snapshot.
     */
    suspend fun createPlaylistFromTracks(
        accessToken: String,
        guildId: String,
        playlistName: String,
        trackUrls: List<String>
    ): Playlist

}
