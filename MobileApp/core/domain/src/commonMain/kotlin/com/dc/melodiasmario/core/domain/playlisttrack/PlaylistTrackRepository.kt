package com.dc.melodiasmario.core.domain.playlisttrack

import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.model.playlist.PlaylistDetail

/**
 * Defines playlist track data operations independently of their concrete data sources.
 */
interface PlaylistTrackRepository {
    /**
     * Retrieves the saved playlist details and ordered tracks for the requested playlist.
     */
    suspend fun getPlaylist(playlistId: String): PlaylistDetail

    /**
     * Removes the track identified by its playlist order number.
     */
    suspend fun removeTrack(playlistId: String, trackNumber: Int): Unit

    /**
     * Requests adding the supplied music query or URL to a saved playlist.
     */
    suspend fun addTrack(playlistId: String, query: String): BotControlCommand

    /**
     * Requests loading the saved playlist into the playback queue.
     */
    suspend fun loadPlaylist(playlistId: String): BotControlCommand
}
