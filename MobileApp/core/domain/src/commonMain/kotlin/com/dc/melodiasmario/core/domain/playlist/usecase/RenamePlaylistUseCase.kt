package com.dc.melodiasmario.core.domain.playlist.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.playlist.PlaylistsRepository
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory

/**
 * Exposes rename playlist as a domain operation.
 */
@Factory
class RenamePlaylistUseCase(
    private val playlistsRepository: PlaylistsRepository
) {
    /**
     * Executes the rename playlist domain operation.
     */
    operator fun invoke(playlistId: String, newPlaylistName: String): Flow<Resource<Unit>> = flow {
        emit(value = Resource.Loading)
        try {
            playlistsRepository.renamePlaylist(playlistId = playlistId, newName = newPlaylistName)
            emit(value = Resource.Success(data = Unit))
        } catch (cancellationException: CancellationException) {
            throw cancellationException
        } catch (exception: Exception) {
            emit(value = Resource.Error(error = exception))
        }
    }
}
