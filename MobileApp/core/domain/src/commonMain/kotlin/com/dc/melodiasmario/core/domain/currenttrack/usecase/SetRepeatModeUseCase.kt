package com.dc.melodiasmario.core.domain.currenttrack.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.currenttrack.CurrentTrackRepository
import com.dc.melodiasmario.core.model.currenttrack.RepeatMode
import kotlin.coroutines.cancellation.CancellationException
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory

/**
 * Exposes set repeat mode as a domain operation.
 */
@Factory
class SetRepeatModeUseCase(
    private val currentTrackRepository: CurrentTrackRepository
) {
    /**
     * Executes the set repeat mode domain operation.
     */
    operator fun invoke(guildId: String, repeatMode: RepeatMode): Flow<Resource<Unit>> = flow {
        emit(value = Resource.Loading)

        try {
            currentTrackRepository.setRepeatMode(guildId = guildId, repeatMode = repeatMode)
            emit(value = Resource.Success(data = Unit))
        } catch (cancellationException: CancellationException) {
            throw cancellationException
        } catch (exception: Exception) {
            emit(value = Resource.Error(error = exception))
        }
    }
}
