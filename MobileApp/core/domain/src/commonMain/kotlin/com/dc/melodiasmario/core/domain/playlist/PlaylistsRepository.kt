package com.dc.melodiasmario.core.domain.playlist

import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.model.playlist.Playlist

/**
 * Defines playlists data operations independently of their concrete data sources.
 */
interface PlaylistsRepository {
    /**
     * Sets the playlist cover URL, or restores automatic artwork when the value is absent.
     */
    suspend fun setArtwork(playlistId: String, artworkUri: String?): Unit

    /**
     * Retrieves the playlists belonging to the requested guild.
     */
    suspend fun getPlaylists(guildId: String): List<Playlist>

    /**
     * Deletes the identified saved playlist.
     */
    suspend fun deletePlaylist(playlistId: String): Unit

    /**
     * Changes the name of the identified playlist.
     */
    suspend fun renamePlaylist(playlistId: String, newName: String): Unit

    /**
     * Creates a named playlist for the requested guild.
     */
    suspend fun createPlaylist(guildId: String, playlistName: String): Playlist

    /**
     * Requests adding the supplied music query or URL to a saved playlist.
     */
    suspend fun addTrack(playlistId: String, songUrl: String): BotControlCommand

    /**
     * Defines the bulk playlist-creation operation for an ordered track snapshot.
     */
    suspend fun createPlaylistFromTracks(
        guildId: String,
        playlistName: String,
        trackUrls: List<String>
    ): Playlist

}
