package com.dc.melodiasmario.core.domain.currenttrack.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.currenttrack.CurrentTrackRepository
import com.dc.melodiasmario.core.domain.guild.GuildRepository
import com.dc.melodiasmario.core.domain.queue.QueueRepository
import com.dc.melodiasmario.core.model.currenttrack.CurrentTrack
import kotlin.coroutines.cancellation.CancellationException
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory

/**
 * Exposes get current track as a domain operation.
 */
@Factory
class GetCurrentTrackUseCase(
    private val currentTrackRepository: CurrentTrackRepository,
    private val queueRepository: QueueRepository,
    private val guildRepository: GuildRepository
) {
    /**
     * Executes the get current track domain operation.
     */
    operator fun invoke(guildId: String): Flow<Resource<CurrentTrack>> = flow {
        emit(value = Resource.Loading)

        try {
            val playbackStatus = currentTrackRepository.getPlaybackStatus(guildId = guildId)
            val queue = queueRepository.getQueue(guildId = guildId)
            val guild = guildRepository.getSelectedGuild(guildId = guildId)
            val data = playbackStatus.toCurrentTrack(queue = queue, guild = guild)

            emit(value = Resource.Success(data = data))
        } catch (cancellationException: CancellationException) {
            throw cancellationException
        } catch (exception: Exception) {
            emit(value = Resource.Error(error = exception))
        }
    }
}
