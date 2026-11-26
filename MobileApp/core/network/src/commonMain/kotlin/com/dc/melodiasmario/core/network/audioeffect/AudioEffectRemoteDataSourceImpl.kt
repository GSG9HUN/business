package com.dc.melodiasmario.core.network.audioeffect

import com.dc.melodiasmario.core.model.audioeffect.AudioEffectsSnapshot
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import org.koin.core.annotation.Single

/** Maps remote audio-effect responses to domain-facing models. */
@Single(binds = [AudioEffectRemoteDataSource::class])
class AudioEffectRemoteDataSourceImpl(
    private val audioEffectApiService: AudioEffectApiService
) : AudioEffectRemoteDataSource {
    override suspend fun getAudioEffects(accessToken: String, guildId: String, page: Int): AudioEffectsSnapshot {
        return audioEffectApiService.getAudioEffects(accessToken = accessToken, guildId = guildId, page = page)
            .toDomain()
    }

    override suspend fun applyAudioEffects(accessToken: String, guildId: String, commands: List<String>): BotControlCommand {
        return audioEffectApiService.applyAudioEffects(accessToken = accessToken, guildId = guildId, commands = commands)
            .toDomain()
    }

    override suspend fun saveAudioEffectPreset(
        accessToken: String,
        guildId: String,
        name: String,
        commands: List<String>,
        overwrite: Boolean
    ): BotControlCommand {
        return audioEffectApiService.saveAudioEffectPreset(
            accessToken = accessToken,
            guildId = guildId,
            name = name,
            commands = commands,
            overwrite = overwrite
        )
            .toDomain()
    }
}
