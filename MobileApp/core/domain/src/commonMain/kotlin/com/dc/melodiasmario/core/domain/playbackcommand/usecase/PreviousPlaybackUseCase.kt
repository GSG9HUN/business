package com.dc.melodiasmario.core.domain.playbackcommand.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.playbackcommand.PlaybackCommandRepository
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory

/**
 * Exposes previous playback as a domain operation.
 */
@Factory
class PreviousPlaybackUseCase(
    private val playbackCommandRepository: PlaybackCommandRepository
) {
    /**
     * Executes the previous playback domain operation.
     */
    operator fun invoke(guildId: String): Flow<Resource<BotControlCommand>> = flow {
        emit(value = Resource.Loading)
        try {
            val previousControlCommand = playbackCommandRepository.previous(guildId = guildId)
            emit(value = Resource.Success(data = previousControlCommand))
        } catch (cancellationException: CancellationException) {
            throw cancellationException
        } catch (exception: Exception) {
            emit(value = Resource.Error(error = exception))
        }
    }
}
