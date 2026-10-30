package com.dc.melodiasmario.core.domain.playlisttrack

import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.model.playlist.PlaylistDetail

interface PlaylistTrackRepository {
    suspend fun getPlaylist(playlistId: String): PlaylistDetail
    suspend fun removeTrack(playlistId: String, trackNumber: Int)
    suspend fun addTrack(playlistId: String, query: String): BotControlCommand
    suspend fun loadPlaylist(playlistId: String): BotControlCommand
}
