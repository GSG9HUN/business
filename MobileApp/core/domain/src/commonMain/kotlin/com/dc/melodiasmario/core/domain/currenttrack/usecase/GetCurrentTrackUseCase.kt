package com.dc.melodiasmario.core.domain.currenttrack.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.currenttrack.CurrentTrackRepository
import com.dc.melodiasmario.core.domain.guild.GuildRepository
import com.dc.melodiasmario.core.domain.queue.QueueRepository
import com.dc.melodiasmario.core.model.currenttrack.CurrentTrack
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Single

@Single
class GetCurrentTrackUseCase(
    private val currentTrackRepository: CurrentTrackRepository,
    private val queueRepository: QueueRepository,
    private val guildRepository: GuildRepository,
) {
    operator fun invoke(guildId: String): Flow<Resource<CurrentTrack>> = flow {
        emit(Resource.Loading)

        try {
            val playbackStatus = currentTrackRepository.getPlaybackStatus(guildId = guildId)
            val queue = queueRepository.getQueue(guildId = guildId)
            val guild = guildRepository.getSelectedGuild(guildId = guildId)

            emit(Resource.Success(playbackStatus.toCurrentTrack(queue = queue, guild = guild)))
        } catch (e: Exception) {
            emit(Resource.Error(e))
        }
    }
}
