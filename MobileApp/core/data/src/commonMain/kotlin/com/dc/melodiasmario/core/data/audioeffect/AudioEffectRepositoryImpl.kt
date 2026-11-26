package com.dc.melodiasmario.core.data.audioeffect

import com.dc.melodiasmario.core.data.auth.AuthorizedSessionProvider
import com.dc.melodiasmario.core.domain.audioeffect.AudioEffectRepository
import com.dc.melodiasmario.core.model.audioeffect.AudioEffectsSnapshot
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.network.audioeffect.AudioEffectRemoteDataSource
import org.koin.core.annotation.Single

/** Provides audio-effect access using a valid authorized session. */
@Single(binds = [AudioEffectRepository::class])
class AudioEffectRepositoryImpl(
    private val audioEffectRemoteDataSource: AudioEffectRemoteDataSource,
    private val authorizedSessionProvider: AuthorizedSessionProvider
) : AudioEffectRepository {
    override suspend fun getAudioEffects(guildId: String, page: Int): AudioEffectsSnapshot {
        val accessToken = authorizedSessionProvider.getValidSession()
        return audioEffectRemoteDataSource.getAudioEffects(accessToken = accessToken, guildId = guildId, page = page)
    }

    override suspend fun applyAudioEffects(guildId: String, commands: List<String>): BotControlCommand {
        val accessToken = authorizedSessionProvider.getValidSession()
        return audioEffectRemoteDataSource.applyAudioEffects(accessToken = accessToken, guildId = guildId, commands = commands)
    }

    override suspend fun saveAudioEffectPreset(
        guildId: String,
        name: String,
        commands: List<String>,
        overwrite: Boolean
    ): BotControlCommand {
        val accessToken = authorizedSessionProvider.getValidSession()
        return audioEffectRemoteDataSource.saveAudioEffectPreset(
            accessToken = accessToken,
            guildId = guildId,
            name = name,
            commands = commands,
            overwrite = overwrite
        )
    }
}
