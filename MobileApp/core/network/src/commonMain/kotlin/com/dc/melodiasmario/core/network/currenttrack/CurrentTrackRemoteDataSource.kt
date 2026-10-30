package com.dc.melodiasmario.core.network.currenttrack

import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.model.currenttrack.PlaybackStatus

/**
 * Defines remote current track data operations.
 */
interface CurrentTrackRemoteDataSource {
    /**
     * Retrieves the guild playback status and current track.
     */
    suspend fun getPlaybackStatus(accessToken: String, guildId: String): PlaybackStatus

    /**
     * Requests advancing to the next queued track.
     */
    suspend fun nextTrack(accessToken: String, guildId: String): BotControlCommand

    /**
     * Requests playing the previous track.
     */
    suspend fun previousTrack(accessToken: String, guildId: String): BotControlCommand

    /**
     * Requests resuming guild playback.
     */
    suspend fun play(accessToken: String, guildId: String): BotControlCommand

    /**
     * Requests pausing guild playback.
     */
    suspend fun pause(accessToken: String, guildId: String): BotControlCommand

    /**
     * Requests updating the playback repeat mode.
     */
    suspend fun setRepeatMode(accessToken: String, guildId: String, mode: String): Unit
}
