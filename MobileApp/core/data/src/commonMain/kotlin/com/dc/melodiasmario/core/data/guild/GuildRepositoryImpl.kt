package com.dc.melodiasmario.core.data.guild

import com.dc.melodiasmario.core.data.auth.AuthorizedSessionProvider
import com.dc.melodiasmario.core.domain.guild.GuildRepository
import com.dc.melodiasmario.core.model.guild.Guild
import com.dc.melodiasmario.core.network.guild.GuildRemoteDataSource
import org.koin.core.annotation.Single

@Single(binds = [GuildRepository::class])
class GuildRepositoryImpl(
    private val guildRemoteDataSource: GuildRemoteDataSource,
    private val authorizedSessionProvider: AuthorizedSessionProvider
) : GuildRepository {
    override suspend fun getGuilds(): List<Guild> {
        val accessToken = authorizedSessionProvider.getValidSession()

        return guildRemoteDataSource
            .getGuilds(accessToken = accessToken)
    }
}