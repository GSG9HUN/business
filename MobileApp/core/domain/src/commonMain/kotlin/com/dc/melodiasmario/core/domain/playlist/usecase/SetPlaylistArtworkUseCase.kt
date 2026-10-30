package com.dc.melodiasmario.core.domain.playlist.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.playlist.PlaylistsRepository
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory

/** Saves a custom cover URL or restores automatic artwork. */
@Factory
class SetPlaylistArtworkUseCase(
    private val playlistTrackRepository: PlaylistsRepository
) {
    operator fun invoke(playlistId: String, artworkUri: String?) = flow {
        emit(Resource.Loading)
        try {
            playlistTrackRepository.setArtwork(playlistId = playlistId, artworkUri = artworkUri)
            emit(Resource.Success(Unit))
        } catch (exception: CancellationException) {
            throw exception
        } catch (exception: Exception) {
            emit(Resource.Error(exception))
        }
    }
}
