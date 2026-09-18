package com.dc.melodiasmario.core.domain.currenttrack

import com.dc.melodiasmario.core.model.currenttrack.PlaybackStatus
import com.dc.melodiasmario.core.model.currenttrack.RepeatMode

interface CurrentTrackRepository {
    suspend fun getPlaybackStatus(guildId: String): PlaybackStatus
    suspend fun nextTrack(guildId: String)
    suspend fun previousTrack(guildId: String)
    suspend fun play(guildId: String)
    suspend fun pause(guildId: String)
    suspend fun setRepeatMode(guildId: String, repeatMode: RepeatMode)
}
