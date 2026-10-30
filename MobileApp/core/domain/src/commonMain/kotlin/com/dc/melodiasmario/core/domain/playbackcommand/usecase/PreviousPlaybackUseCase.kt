package com.dc.melodiasmario.core.domain.playbackcommand.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.playbackcommand.PlaybackCommandRepository
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory

@Factory
class PreviousPlaybackUseCase(
    private val playBackCommandRepository: PlaybackCommandRepository
) {
    operator fun invoke(guildId: String): Flow<Resource<BotControlCommand>> = flow {
        emit(Resource.Loading)
        try {
            val previousControlCommand = playBackCommandRepository.previous(guildId = guildId)
            emit(Resource.Success(previousControlCommand))
        } catch (e: Exception) {
            emit(Resource.Error(e))
        }
    }
}