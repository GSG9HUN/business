package com.dc.melodiasmario.core.domain.currenttrack

import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.model.currenttrack.PlaybackStatus
import com.dc.melodiasmario.core.model.currenttrack.RepeatMode

interface CurrentTrackRepository {
    suspend fun getPlaybackStatus(guildId: String): PlaybackStatus
    suspend fun nextTrack(guildId: String): BotControlCommand
    suspend fun previousTrack(guildId: String): BotControlCommand
    suspend fun play(guildId: String): BotControlCommand
    suspend fun pause(guildId: String): BotControlCommand
    suspend fun setRepeatMode(guildId: String, repeatMode: RepeatMode)
}
