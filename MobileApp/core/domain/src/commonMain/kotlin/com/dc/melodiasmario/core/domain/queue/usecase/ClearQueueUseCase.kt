package com.dc.melodiasmario.core.domain.queue.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.queue.QueueRepository
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import kotlin.coroutines.cancellation.CancellationException
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory

/**
 * Exposes clear queue as a domain operation.
 */
@Factory
class ClearQueueUseCase(
    private val queueRepository: QueueRepository
) {
    /**
     * Executes the clear queue domain operation.
     */
    operator fun invoke(guildId: String): Flow<Resource<BotControlCommand>> = flow {
        emit(value = Resource.Loading)

        try {
            val clearQueueControlCommand = queueRepository.clearQueue(guildId = guildId)
            emit(value = Resource.Success(data = clearQueueControlCommand))
        } catch (cancellationException: CancellationException) {
            throw cancellationException
        } catch (exception: Exception) {
            emit(value = Resource.Error(error = exception))
        }
    }
}
