package com.dc.melodiasmario.core.domain.audioeffect.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.audioeffect.AudioEffectRepository
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import kotlin.coroutines.cancellation.CancellationException
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory

/** Coordinates preset persistence through its dedicated repository operation while preserving cancellation. */
@Factory
class SaveAudioEffectPresetUseCase(private val audioEffectRepository: AudioEffectRepository) {

    /** Emits command acceptance for a save-only request; no live audio settings are applied. */
    operator fun invoke(
        guildId: String,
        name: String,
        commands: List<String>,
        overwrite: Boolean
    ): Flow<Resource<BotControlCommand>> = flow {
        emit(value = Resource.Loading)

        try {
            val command = audioEffectRepository.saveAudioEffectPreset(
                guildId = guildId,
                name = name,
                commands = commands,
                overwrite = overwrite
            )
            emit(value = Resource.Success(data = command))
        } catch (cancellationException: CancellationException) {
            throw cancellationException
        } catch (exception: Exception) {
            emit(value = Resource.Error(error = exception))
        }
    }
}
