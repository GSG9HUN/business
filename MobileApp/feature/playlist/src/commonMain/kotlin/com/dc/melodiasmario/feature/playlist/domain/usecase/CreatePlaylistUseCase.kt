package com.dc.melodiasmario.feature.playlist.domain.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.feature.playlist.domain.repository.PlaylistsRepository
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Single

@Single
class CreatePlaylistUseCase(
    private val playlistsRepository: PlaylistsRepository
) {
    operator fun invoke(guildId: String, playlistName: String): Flow<Resource<Unit>> = flow {
        emit(Resource.Loading)
        try {
            playlistsRepository.createPlaylist(guildId = guildId, playlistName = playlistName)
            emit(Resource.Success(Unit))
        } catch (e: Exception) {
            emit(Resource.Error(e))
        }
    }
}