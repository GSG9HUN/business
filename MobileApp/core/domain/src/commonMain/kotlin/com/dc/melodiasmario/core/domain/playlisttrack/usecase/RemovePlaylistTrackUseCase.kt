package com.dc.melodiasmario.core.domain.playlisttrack.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.playlisttrack.PlaylistTrackRepository
import kotlin.coroutines.cancellation.CancellationException
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory

/**
 * Exposes remove playlist track as a domain operation.
 */
@Factory
class RemovePlaylistTrackUseCase(
    private val playlistTrackRepository: PlaylistTrackRepository
) {
    /**
     * Executes the remove playlist track domain operation.
     */
    operator fun invoke(playlistId: String, trackNumber: Int): Flow<Resource<Unit>> = flow {
        emit(value = Resource.Loading)
        try {
            playlistTrackRepository.removeTrack(playlistId = playlistId, trackNumber = trackNumber)
            emit(value = Resource.Success(data = Unit))
        } catch (error: CancellationException) {
            throw error
        } catch (error: Exception) {
            emit(value = Resource.Error(error = error))
        }
    }
}
