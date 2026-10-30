package com.dc.melodiasmario.core.domain.playlisttrack.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.playlisttrack.PlaylistTrackRepository
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory
import kotlin.coroutines.cancellation.CancellationException

@Factory
class LoadPlaylistUseCase(
    private val playlistTrackRepository: PlaylistTrackRepository
) {
    operator fun invoke(playlistId: String): Flow<Resource<BotControlCommand>> = flow {
        emit(Resource.Loading)
        try {
            val loadPlaylistControlCommand = playlistTrackRepository.loadPlaylist(playlistId = playlistId)
            emit(Resource.Success(loadPlaylistControlCommand))
        } catch (error: CancellationException) {
            throw error
        } catch (error: Exception) {
            emit(Resource.Error(error))
        }
    }
}
