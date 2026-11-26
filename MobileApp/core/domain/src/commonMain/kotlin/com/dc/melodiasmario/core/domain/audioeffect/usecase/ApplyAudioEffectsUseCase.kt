package com.dc.melodiasmario.core.domain.audioeffect.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.audioeffect.AudioEffectRepository
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import kotlin.coroutines.cancellation.CancellationException
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory

/** Coordinates guild audio effects through the authorized repository while preserving cancellation. */
@Factory
class ApplyAudioEffectsUseCase(private val audioEffectRepository: AudioEffectRepository) {

    /** Emits loading, the server result, or a recoverable error; command acceptance is not audio completion. */
    operator fun invoke(guildId: String, commands: List<String>): Flow<Resource<BotControlCommand>> = flow {
        emit(value = Resource.Loading)

        try {
            emit(value = Resource.Success(data = audioEffectRepository.applyAudioEffects(guildId = guildId, commands = commands)))
        } catch (cancellationException: CancellationException) {
            throw cancellationException
        } catch (exception: Exception) {
            emit(value = Resource.Error(error = exception))
        }
    }
}
