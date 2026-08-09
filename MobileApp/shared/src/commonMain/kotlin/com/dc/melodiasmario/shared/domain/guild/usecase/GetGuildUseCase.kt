package com.dc.melodiasmario.shared.domain.guild.usecase

import com.dc.melodiasmario.shared.core.Resource
import com.dc.melodiasmario.shared.domain.guild.model.Guild
import com.dc.melodiasmario.shared.domain.guild.repository.GuildRepository
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
