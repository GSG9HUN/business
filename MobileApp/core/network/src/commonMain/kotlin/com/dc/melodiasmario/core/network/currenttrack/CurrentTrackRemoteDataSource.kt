package com.dc.melodiasmario.core.network.currenttrack

import com.dc.melodiasmario.core.model.currenttrack.PlaybackStatus

interface CurrentTrackRemoteDataSource {
    suspend fun getPlaybackStatus(accessToken: String, guildId: String): PlaybackStatus
    suspend fun nextTrack(accessToken: String, guildId: String)
    suspend fun previousTrack(accessToken: String, guildId: String)
    suspend fun play(accessToken: String, guildId: String)
    suspend fun pause(accessToken: String, guildId: String)
    suspend fun setRepeatMode(accessToken: String, guildId: String, mode: String)
}
