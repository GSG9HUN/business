package com.dc.melodiasmario.core.domain.playlist.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.playlist.PlaylistsRepository
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory

/** Saves a custom cover URL or restores automatic artwork. */
@Factory
class SetPlaylistArtworkUseCase(
    private val playlistsRepository: PlaylistsRepository
) {
    /**
     * Executes the set playlist artwork domain operation.
     */
    operator fun invoke(playlistId: String, artworkUri: String?): Flow<Resource<Unit>> = flow {
        emit(value = Resource.Loading)
        try {
            playlistsRepository.setArtwork(playlistId = playlistId, artworkUri = artworkUri)
            emit(value = Resource.Success(data = Unit))
        } catch (exception: CancellationException) {
            throw exception
        } catch (exception: Exception) {
            emit(value = Resource.Error(error = exception))
        }
    }
}
