package com.dc.melodiasmario.core.domain.currenttrack.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.currenttrack.CurrentTrackRepository
import com.dc.melodiasmario.core.model.currenttrack.RepeatMode
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory
import kotlin.coroutines.cancellation.CancellationException

@Factory
class SetRepeatModeUseCase(
    private val currentTrackRepository: CurrentTrackRepository
) {
    operator fun invoke(guildId: String, repeatMode: RepeatMode): Flow<Resource<Unit>> = flow {
        emit(Resource.Loading)

        try {
            currentTrackRepository.setRepeatMode(guildId = guildId, repeatMode = repeatMode)
            emit(Resource.Success(Unit))
        } catch (e: CancellationException) {
            throw e
        } catch (e: Exception) {
            emit(Resource.Error(e))
        }
    }
}
