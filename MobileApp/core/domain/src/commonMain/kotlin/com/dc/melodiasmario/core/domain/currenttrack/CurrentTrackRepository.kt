package com.dc.melodiasmario.core.domain.currenttrack

import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.model.currenttrack.PlaybackStatus
import com.dc.melodiasmario.core.model.currenttrack.RepeatMode

/**
 * Defines current track data operations independently of their concrete data sources.
 */
interface CurrentTrackRepository {
    /**
     * Retrieves the guild playback status and current track.
     */
    suspend fun getPlaybackStatus(guildId: String): PlaybackStatus

    /**
     * Requests advancing to the next queued track.
     */
    suspend fun nextTrack(guildId: String): BotControlCommand

    /**
     * Requests playing the previous track.
     */
    suspend fun previousTrack(guildId: String): BotControlCommand

    /**
     * Requests resuming guild playback.
     */
    suspend fun play(guildId: String): BotControlCommand

    /**
     * Requests pausing guild playback.
     */
    suspend fun pause(guildId: String): BotControlCommand

    /**
     * Requests updating the playback repeat mode.
     */
    suspend fun setRepeatMode(guildId: String, repeatMode: RepeatMode): Unit
}
