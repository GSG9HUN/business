package com.dc.melodiasmario.core.domain.queue.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.queue.QueueRepository
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import kotlin.coroutines.cancellation.CancellationException
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory

/**
 * Exposes move track to index as a domain operation.
 */
@Factory
class MoveTrackToIndexUseCase(
    private val queueRepository: QueueRepository
) {
    /**
     * Executes the move track to index domain operation.
     */
    operator fun invoke(
        guildId: String,
        trackIndex: Int,
        targetIndex: Int
    ): Flow<Resource<BotControlCommand>> = flow {
        emit(value = Resource.Loading)

        try {
            val moveTrackToIndexControlCommand = queueRepository.moveTrackToIndex(
                guildId = guildId,
                trackIndex = trackIndex,
                targetIndex = targetIndex
            )
            emit(value = Resource.Success(data = moveTrackToIndexControlCommand))
        } catch (cancellationException: CancellationException) {
            throw cancellationException
        } catch (exception: Exception) {
            emit(value = Resource.Error(error = exception))
        }
    }
}
