package com.dc.melodiasmario.core.domain.queue.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.queue.QueueRepository
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory
import kotlin.coroutines.cancellation.CancellationException

@Factory
class RemoveFromQueueUseCase(
    private val queueRepository: QueueRepository
) {
    operator fun invoke(guildId: String, trackNumber: String): Flow<Resource<BotControlCommand>> =
        flow {
            emit(Resource.Loading)

            try {
                emit(
                    Resource.Success(
                        queueRepository.removeFromQueue(
                            guildId = guildId,
                            trackNumber = trackNumber
                        )
                    )
                )
            } catch (e: CancellationException) {
                throw e
            } catch (e: Exception) {
                emit(Resource.Error(e))
            }
        }
}
