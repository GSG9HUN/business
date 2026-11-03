package com.dc.melodiasmario.core.domain.playlisttrack.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.playlisttrack.PlaylistTrackRepository
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import kotlin.coroutines.cancellation.CancellationException
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory

/**
 * Exposes add playlist track as a domain operation.
 */
@Factory
class AddPlaylistTrackUseCase(
    private val playlistTrackRepository: PlaylistTrackRepository
) {
    /**
     * Executes the add playlist track domain operation.
     */
    operator fun invoke(playlistId: String, query: String): Flow<Resource<BotControlCommand>> =
        flow {
            emit(value = Resource.Loading)
            try {
                val addTrackControlCommand = playlistTrackRepository.addTrack(playlistId = playlistId, query = query)
                emit(value = Resource.Success(data = addTrackControlCommand))
            } catch (error: CancellationException) {
                throw error
            } catch (error: Exception) {
                emit(value = Resource.Error(error = error))
            }
        }
}
