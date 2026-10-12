package com.dc.melodiasmario.core.domain.playbackcommand

import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand

interface PlaybackCommandRepository {
    suspend fun previous(guildId: String): BotControlCommand
    suspend fun next(guildId: String): BotControlCommand
    suspend fun playPause(guildId: String, isPlaying: Boolean): BotControlCommand
}