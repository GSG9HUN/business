package com.dc.melodiasmario.feature.guild.domain.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.feature.guild.domain.model.Guild
import com.dc.melodiasmario.feature.guild.domain.repository.GuildRepository
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Single

@Single
class GetGuildUseCase(
    private val guildRepository: GuildRepository,
) {
    operator fun invoke(): Flow<Resource<List<Guild>>> = flow {
        emit(Resource.Loading)

        try {
            val guilds = guildRepository.getGuilds()
            emit(Resource.Success(guilds))
        } catch (e: Exception) {
            emit(Resource.Error(e))
        }
    }
}
