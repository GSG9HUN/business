package com.dc.melodiasmario.core.domain.queue.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.queue.QueueRepository
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import kotlin.coroutines.cancellation.CancellationException
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory

/**
 * Exposes add to queue as a domain operation.
 */
@Factory
class AddToQueueUseCase(
    private val queueRepository: QueueRepository
) {
    /**
     * Executes the add to queue domain operation.
     */
    operator fun invoke(guildId:String, query: String): Flow<Resource<BotControlCommand>> = flow {
        emit(value = Resource.Loading)

        try {
            val addToQueueControlCommand = queueRepository.addToQueue(guildId = guildId, query = query)
            emit(value = Resource.Success(data = addToQueueControlCommand))
        } catch (cancellationException: CancellationException) {
            throw cancellationException
        } catch (exception: Exception) {
            emit(value = Resource.Error(error = exception))
        }
    }
}
