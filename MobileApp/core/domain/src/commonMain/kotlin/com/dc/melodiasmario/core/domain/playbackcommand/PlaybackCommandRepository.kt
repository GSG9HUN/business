package com.dc.melodiasmario.core.domain.playbackcommand

import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand

/**
 * Defines playback command data operations independently of their concrete data sources.
 */
interface PlaybackCommandRepository {
    /**
     * Requests the previous playback track.
     */
    suspend fun previous(guildId: String): BotControlCommand

    /**
     * Requests the next playback track.
     */
    suspend fun next(guildId: String): BotControlCommand

    /**
     * Requests pausing or resuming playback according to its current state.
     */
    suspend fun playPause(guildId: String, isPlaying: Boolean): BotControlCommand
}
