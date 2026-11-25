package com.dc.melodiasmario.core.domain.currenttrack.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.currenttrack.CurrentTrackRepository
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import kotlin.coroutines.cancellation.CancellationException
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory

/** Accepts one seek command while preserving cancellation and the captured playback identity. */
@Factory
class SeekTrackUseCase(
    private val currentTrackRepository: CurrentTrackRepository
) {
    /** Emits request loading and acceptance/error; terminal completion is observed separately. */
    operator fun invoke(
        guildId: String,
        positionSeconds: Int,
        expectedPlaybackOperationId: String
    ): Flow<Resource<BotControlCommand>> = flow {
        emit(value = Resource.Loading)

        try {
            require(value = positionSeconds >= 0 && expectedPlaybackOperationId.isNotBlank())
            val seekCommand = currentTrackRepository.seek(
                guildId = guildId,
                positionSeconds = positionSeconds,
                expectedPlaybackOperationId = expectedPlaybackOperationId
            )
            emit(value = Resource.Success(data = seekCommand))
        } catch (cancellationException: CancellationException) {
            throw cancellationException
        } catch (exception: Exception) {
            emit(value = Resource.Error(error = exception))
        }
    }
}
