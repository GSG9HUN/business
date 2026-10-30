package com.dc.melodiasmario.core.domain.playlisttrack.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.playlisttrack.PlaylistTrackRepository

import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory
import kotlin.coroutines.cancellation.CancellationException

@Factory
class RemovePlaylistTrackUseCase(
    private val playlistTrackRepository: PlaylistTrackRepository
) {
    operator fun invoke(playlistId: String, trackNumber: Int): Flow<Resource<Unit>> = flow {
        emit(Resource.Loading)
        try {
            playlistTrackRepository.removeTrack(playlistId = playlistId, trackNumber = trackNumber)
            emit(Resource.Success(Unit))
        } catch (error: CancellationException) {
            throw error
        } catch (error: Exception) {
            emit(Resource.Error(error))
        }
    }
}
