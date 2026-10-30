package com.dc.melodiasmario.core.domain.guild.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.guild.GuildRepository
import com.dc.melodiasmario.core.model.guild.Guild
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory

/**
 * Exposes get guild as a domain operation.
 */
@Factory
class GetGuildUseCase(
    private val guildRepository: GuildRepository
) {
    /**
     * Executes the get guild domain operation.
     */
    operator fun invoke(): Flow<Resource<List<Guild>>> = flow {
        emit(value = Resource.Loading)

        try {
            val guilds = guildRepository.getGuilds()
            emit(value = Resource.Success(data = guilds))
        } catch (cancellationException: CancellationException) {
            throw cancellationException
        } catch (exception: Exception) {
            emit(value = Resource.Error(error = exception))
        }
    }
}
