package com.dc.melodiasmario.core.domain.playlist.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.playlist.PlaylistsRepository
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory

/**
 * Exposes delete playlist as a domain operation.
 */
@Factory
class DeletePlaylistUseCase(
    private val playlistsRepository: PlaylistsRepository
) {
    /**
     * Executes the delete playlist domain operation.
     */
    operator fun invoke(playlistId: String): Flow<Resource<Unit>> = flow {
        emit(value = Resource.Loading)
        try {
            playlistsRepository.deletePlaylist(playlistId = playlistId)
            emit(value = Resource.Success(data = Unit))
        } catch (cancellationException: CancellationException) {
            throw cancellationException
        } catch (exception: Exception) {
            emit(value = Resource.Error(error = exception))
        }
    }
}
