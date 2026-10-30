package com.dc.melodiasmario.core.domain.playlist.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.playlist.PlaylistsRepository
import com.dc.melodiasmario.core.model.playlist.Playlist
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory

/**
 * Exposes get playlists as a domain operation.
 */
@Factory
class GetPlaylistsUseCase(
    private val playlistsRepository: PlaylistsRepository
) {
    /**
     * Executes the get playlists domain operation.
     */
    operator fun invoke(guildId: String): Flow<Resource<List<Playlist>>> = flow{
        emit(value = Resource.Loading)
        try {
            val playlists = playlistsRepository.getPlaylists(guildId = guildId)
            emit(value = Resource.Success(data = playlists))
        } catch (cancellationException: CancellationException) {
            throw cancellationException
        } catch (exception: Exception) {
            emit(value = Resource.Error(error = exception))
        }
    }
}
