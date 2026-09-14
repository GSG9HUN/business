package com.dc.melodiasmario.core.domain.playlist.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.model.playlist.Playlist
import com.dc.melodiasmario.core.domain.playlist.PlaylistsRepository
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