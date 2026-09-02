package com.dc.melodiasmario.feature.playlist.domain.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.feature.playlist.domain.model.Playlist
import com.dc.melodiasmario.feature.playlist.domain.repository.PlaylistsRepository
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Single

@Single
class GetPlaylistsUseCase(
    private val playlistsRepository: PlaylistsRepository
) {
    operator fun invoke(guildId: String): Flow<Resource<List<Playlist>>> = flow{
        emit(Resource.Loading)
        try {
            val playlists = playlistsRepository.getPlaylists(guildId = guildId)
            emit(Resource.Success(playlists))
        } catch (e: Exception) {
            emit(Resource.Error(e))
        }
    }
}