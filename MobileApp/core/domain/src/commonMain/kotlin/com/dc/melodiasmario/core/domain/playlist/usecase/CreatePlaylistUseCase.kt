package com.dc.melodiasmario.core.domain.playlist.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.playlist.PlaylistsRepository
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory

/**
 * Exposes create playlist as a domain operation.
 */
@Factory
class CreatePlaylistUseCase(
    private val playlistsRepository: PlaylistsRepository
) {
    /**
     * Executes the create playlist domain operation.
     */
    operator fun invoke(guildId: String, playlistName: String): Flow<Resource<Unit>> = flow {
        emit(value = Resource.Loading)
        try {
            playlistsRepository.createPlaylist(guildId = guildId, playlistName = playlistName)
            emit(value = Resource.Success(data = Unit))
        } catch (cancellationException: CancellationException) {
            throw cancellationException
        } catch (exception: Exception) {
            emit(value = Resource.Error(error = exception))
        }
    }
}
