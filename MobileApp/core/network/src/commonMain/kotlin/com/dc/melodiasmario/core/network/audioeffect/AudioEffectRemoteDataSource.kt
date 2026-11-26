package com.dc.melodiasmario.core.network.audioeffect

import com.dc.melodiasmario.core.model.audioeffect.AudioEffectsSnapshot
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand

/** Defines remote access to confirmed guild effects and preset operations. */
interface AudioEffectRemoteDataSource {
    /** Reads confirmed effects and guild-local preset metadata. */
    suspend fun getAudioEffects(accessToken: String, guildId: String, page: Int): AudioEffectsSnapshot

    /** Accepts effect operations; preset saves use their dedicated operation. */
    suspend fun applyAudioEffects(accessToken: String, guildId: String, commands: List<String>): BotControlCommand

    /** Saves a named draft without changing live playback; completion is observed separately. */
    suspend fun saveAudioEffectPreset(
        accessToken: String,
        guildId: String,
        name: String,
        commands: List<String>,
        overwrite: Boolean
    ): BotControlCommand
}
