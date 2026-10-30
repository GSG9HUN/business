package com.dc.melodiasmario.core.domain.playbackcommand.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.playbackcommand.PlaybackCommandRepository
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory

@Factory
class NextPlaybackUseCase(
    private val playBackCommandRepository: PlaybackCommandRepository
) {
    operator fun invoke(guildId: String): Flow<Resource<BotControlCommand>> = flow {
        emit(Resource.Loading)
        try {
            val nextControlCommand = playBackCommandRepository.next(guildId = guildId)
            emit(Resource.Success(nextControlCommand))
        } catch (e: Exception) {
            emit(Resource.Error(e))
        }
    }
}