package com.dc.melodiasmario.core.domain.playlisttrack.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.playlisttrack.PlaylistTrackRepository
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory
import kotlin.coroutines.cancellation.CancellationException

@Factory
class AddPlaylistTrackUseCase(
    private val playlistTrackRepository: PlaylistTrackRepository
) {
    operator fun invoke(playlistId: String, query: String): Flow<Resource<BotControlCommand>> =
        flow {
            emit(Resource.Loading)
            try {
                val addTrackControlCommand = playlistTrackRepository.addTrack(playlistId = playlistId, query = query)
                emit(Resource.Success(addTrackControlCommand))
            } catch (error: CancellationException) {
                throw error
            } catch (error: Exception) {
                emit(Resource.Error(error))
            }
        }
}
