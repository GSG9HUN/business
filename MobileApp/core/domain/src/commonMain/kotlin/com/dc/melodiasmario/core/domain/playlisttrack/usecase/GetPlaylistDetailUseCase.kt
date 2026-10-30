package com.dc.melodiasmario.core.domain.playlisttrack.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.playlisttrack.PlaylistTrackRepository
import com.dc.melodiasmario.core.model.playlist.PlaylistDetail
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory
import kotlin.coroutines.cancellation.CancellationException

@Factory
class GetPlaylistDetailUseCase(
    private val playlistTrackRepository: PlaylistTrackRepository
) {
    operator fun invoke(playlistId: String): Flow<Resource<PlaylistDetail>> = flow {
        emit(Resource.Loading)
        try {
            val playlistDetail = playlistTrackRepository.getPlaylist(playlistId = playlistId)
            emit(Resource.Success(playlistDetail))
        } catch (error: CancellationException) {
            throw error
        } catch (error: Exception) {
            emit(Resource.Error(error))
        }
    }
}
