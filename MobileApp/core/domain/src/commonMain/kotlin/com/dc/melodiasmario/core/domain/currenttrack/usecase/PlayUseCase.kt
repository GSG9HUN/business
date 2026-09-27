package com.dc.melodiasmario.core.domain.currenttrack.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.currenttrack.CurrentTrackRepository
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Single

@Single
class PlayUseCase(
    private val currentTrackRepository: CurrentTrackRepository
) {
    operator fun invoke(guildId: String): Flow<Resource<BotControlCommand>> = flow {
        emit(Resource.Loading)

        try {
            emit(Resource.Success(currentTrackRepository.play(guildId = guildId)))
        } catch (e: Exception) {
            emit(Resource.Error(e))
        }
    }
}
