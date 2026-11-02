package com.dc.melodiasmario.core.network.playlisttrack

import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.model.playlist.PlaylistDetail

/**
 * Defines remote playlist track data operations.
 */
interface PlaylistTrackRemoteDataSource {
    /**
     * Retrieves the saved playlist details and ordered tracks for the requested playlist.
     */
    suspend fun getPlaylist(accessToken: String, playlistId: String): PlaylistDetail

    /**
     * Removes the track identified by its playlist order number.
     */
    suspend fun removeTrack(accessToken: String, playlistId: String, trackNumber: Int): Unit

    /**
     * Requests loading the saved playlist into the playback queue.
     */
    suspend fun loadPlaylist(accessToken: String, playlistId: String): BotControlCommand
}
