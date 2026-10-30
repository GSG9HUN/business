package com.dc.melodiasmario.core.domain.playlisttrack.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.playlisttrack.PlaylistTrackRepository
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import kotlin.coroutines.cancellation.CancellationException
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory

/**
 * Exposes load playlist as a domain operation.
 */
@Factory
class LoadPlaylistUseCase(
    private val playlistTrackRepository: PlaylistTrackRepository
) {
    /**
     * Executes the load playlist domain operation.
     */
    operator fun invoke(playlistId: String): Flow<Resource<BotControlCommand>> = flow {
        emit(value = Resource.Loading)
        try {
            val loadPlaylistControlCommand = playlistTrackRepository.loadPlaylist(playlistId = playlistId)
            emit(value = Resource.Success(data = loadPlaylistControlCommand))
        } catch (error: CancellationException) {
            throw error
        } catch (error: Exception) {
            emit(value = Resource.Error(error = error))
        }
    }
}
