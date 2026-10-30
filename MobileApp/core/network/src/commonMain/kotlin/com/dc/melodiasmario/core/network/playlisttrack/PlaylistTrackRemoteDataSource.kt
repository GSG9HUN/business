package com.dc.melodiasmario.core.network.playlisttrack

import com.dc.melodiasmario.core.model.playlist.PlaylistDetail
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand

interface PlaylistTrackRemoteDataSource {
    suspend fun getPlaylist(accessToken: String, playlistId: String): PlaylistDetail
    suspend fun removeTrack(accessToken: String, playlistId: String, trackNumber: Int)
    suspend fun loadPlaylist(accessToken: String, playlistId: String): BotControlCommand
}
