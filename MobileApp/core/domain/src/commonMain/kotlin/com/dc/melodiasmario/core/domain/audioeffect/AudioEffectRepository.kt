package com.dc.melodiasmario.core.domain.audioeffect

import com.dc.melodiasmario.core.model.audioeffect.AudioEffectsSnapshot
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand

/** Defines authorized audio-effect access independently of its data sources. */
interface AudioEffectRepository {
    /** Reads confirmed effects and guild-local preset metadata. */
    suspend fun getAudioEffects(guildId: String, page: Int): AudioEffectsSnapshot

    /** Accepts effect operations; preset saves use their dedicated operation. */
    suspend fun applyAudioEffects(guildId: String, commands: List<String>): BotControlCommand

    /** Persists a named draft without applying it to live playback. */
    suspend fun saveAudioEffectPreset(
        guildId: String,
        name: String,
        commands: List<String>,
        overwrite: Boolean
    ): BotControlCommand
}
