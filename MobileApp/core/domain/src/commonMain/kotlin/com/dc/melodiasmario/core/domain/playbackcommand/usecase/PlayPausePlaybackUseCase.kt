package com.dc.melodiasmario.core.domain.playbackcommand.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.playbackcommand.PlaybackCommandRepository
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Single

@Single
class PlayPausePlaybackUseCase(
    private val repository: PlaybackCommandRepository,
) {
    operator fun invoke(
        guildId: String,
        isPlaying: Boolean,
    ): Flow<Resource<BotControlCommand>> = flow {
        emit(Resource.Loading)
        try {
            emit(Resource.Success(repository.playPause(guildId, isPlaying)))
        } catch (e: Exception) {
            emit(Resource.Error(e))
        }
    }
}