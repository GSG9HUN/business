package com.dc.melodiasmario.core.domain.queue.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.queue.QueueRepository
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Single

@Single
class AddToQueueUseCase(
    private val queueRepository: QueueRepository
) {
    operator fun invoke(guildId:String, query: String): Flow<Resource<BotControlCommand>> = flow {
        emit(Resource.Loading)

        try {
            emit(Resource.Success(queueRepository.addToQueue(guildId = guildId, query = query)))
        } catch (e: Exception) {
            emit(Resource.Error(e))
        }
    }
}
