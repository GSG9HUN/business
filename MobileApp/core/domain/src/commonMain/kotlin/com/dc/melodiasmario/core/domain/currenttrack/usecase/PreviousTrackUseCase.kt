package com.dc.melodiasmario.core.domain.currenttrack.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.currenttrack.CurrentTrackRepository
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory
import kotlin.coroutines.cancellation.CancellationException

@Factory
class PreviousTrackUseCase(
    private val currentTrackRepository: CurrentTrackRepository
) {
    operator fun invoke(guildId: String): Flow<Resource<BotControlCommand>> = flow {
        emit(Resource.Loading)

        try {
            emit(Resource.Success(currentTrackRepository.previousTrack(guildId = guildId)))
        } catch (e: CancellationException) {
            throw e
        } catch (e: Exception) {
            emit(Resource.Error(e))
        }
    }
}
