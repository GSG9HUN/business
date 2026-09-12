package com.dc.melodiasmario.core.domain.playlist.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.playlist.PlaylistsRepository
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Single

@Single
class DeletePlaylistUseCase(
    private val playlistsRepository: PlaylistsRepository
) {
    operator fun invoke(playlistId: String): Flow<Resource<Unit>> = flow {
        emit(Resource.Loading)
        try {
            playlistsRepository.deletePlaylist(playlistId = playlistId)
            emit(Resource.Success(Unit))
        } catch (e: Exception) {
            emit(Resource.Error(e))
        }
    }
}