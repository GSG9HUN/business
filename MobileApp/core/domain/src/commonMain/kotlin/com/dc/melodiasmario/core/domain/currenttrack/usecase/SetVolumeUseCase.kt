package com.dc.melodiasmario.core.domain.currenttrack.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.currenttrack.CurrentTrackRepository
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import kotlin.coroutines.cancellation.CancellationException
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory

/** Accepts absolute output-volume changes while preserving coroutine cancellation. */
@Factory
class SetVolumeUseCase(private val currentTrackRepository: CurrentTrackRepository) {

    /** Emits loading and command acceptance or error; acceptance does not confirm the audio change. */
    operator fun invoke(guildId: String, volumePercent: Int): Flow<Resource<BotControlCommand>> = flow {
        emit(value = Resource.Loading)

        try {
            require(value = volumePercent in 0..200)
            val volumeCommand = currentTrackRepository.setVolume(guildId = guildId, volumePercent = volumePercent)
            emit(value = Resource.Success(data = volumeCommand))
        } catch (cancellationException: CancellationException) {
            throw cancellationException
        } catch (exception: Exception) {
            emit(value = Resource.Error(error = exception))
        }
    }
}
