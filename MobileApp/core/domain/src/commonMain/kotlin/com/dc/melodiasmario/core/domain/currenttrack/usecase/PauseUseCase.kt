package com.dc.melodiasmario.core.domain.currenttrack.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.currenttrack.CurrentTrackRepository
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import kotlin.coroutines.cancellation.CancellationException
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory

/**
 * Exposes pause as a domain operation.
 */
@Factory
class PauseUseCase(
    private val currentTrackRepository: CurrentTrackRepository
) {
    /**
     * Executes the pause domain operation.
     */
    operator fun invoke(guildId: String): Flow<Resource<BotControlCommand>> = flow {
        emit(value = Resource.Loading)

        try {
            val pauseControlCommand = currentTrackRepository.pause(guildId = guildId)
            emit(value = Resource.Success(data = pauseControlCommand))
        } catch (cancellationException: CancellationException) {
            throw cancellationException
        } catch (exception: Exception) {
            emit(value = Resource.Error(error = exception))
        }
    }
}
