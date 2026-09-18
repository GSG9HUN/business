package com.dc.melodiasmario.core.domain.queue.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.queue.QueueRepository
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Single

@Single
class AddToQueueUseCase(
    private val queueRepository: QueueRepository
) {
    operator fun invoke(guildId:String, query: String): Flow<Resource<Unit>> = flow {
        emit(Resource.Loading)

        try {
            queueRepository.addToQueue(guildId = guildId, query = query)
            emit(Resource.Success(Unit))
        } catch (e: Exception) {
            emit(Resource.Error(e))
        }
    }
}