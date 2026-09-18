package com.dc.melodiasmario.core.domain.currenttrack.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.currenttrack.CurrentTrackRepository
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Single

@Single
class PreviousTrackUseCase(
    private val currentTrackRepository: CurrentTrackRepository
) {
    operator fun invoke(guildId: String): Flow<Resource<Unit>> = flow {
        emit(Resource.Loading)

        try {
            currentTrackRepository.previousTrack(guildId = guildId)
            emit(Resource.Success(Unit))
        } catch (e: Exception) {
            emit(Resource.Error(e))
        }
    }
}