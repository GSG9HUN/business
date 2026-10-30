package com.dc.melodiasmario.core.domain.playbackcommand.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.playbackcommand.PlaybackCommandRepository
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory

/**
 * Exposes play pause playback as a domain operation.
 */
@Factory
class PlayPausePlaybackUseCase(
    private val playbackCommandRepository: PlaybackCommandRepository
) {
    /**
     * Executes the play pause playback domain operation.
     */
    operator fun invoke(
        guildId: String,
        isPlaying: Boolean
    ): Flow<Resource<BotControlCommand>> = flow {
        emit(value = Resource.Loading)
        try {
            val playPauseControlCommand =
                playbackCommandRepository.playPause(guildId = guildId, isPlaying = isPlaying)
            emit(value = Resource.Success(data = playPauseControlCommand))
        } catch (cancellationException: CancellationException) {
            throw cancellationException
        } catch (exception: Exception) {
            emit(value = Resource.Error(error = exception))
        }
    }
}
