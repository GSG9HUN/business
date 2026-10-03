package com.dc.melodiasmario.core.network.currenttrack

import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.model.currenttrack.PlaybackStatus

interface CurrentTrackRemoteDataSource {
    suspend fun getPlaybackStatus(accessToken: String, guildId: String): PlaybackStatus
    suspend fun nextTrack(accessToken: String, guildId: String): BotControlCommand
    suspend fun previousTrack(accessToken: String, guildId: String): BotControlCommand
    suspend fun play(accessToken: String, guildId: String): BotControlCommand
    suspend fun pause(accessToken: String, guildId: String): BotControlCommand
    suspend fun setRepeatMode(accessToken: String, guildId: String, mode: String)
}
