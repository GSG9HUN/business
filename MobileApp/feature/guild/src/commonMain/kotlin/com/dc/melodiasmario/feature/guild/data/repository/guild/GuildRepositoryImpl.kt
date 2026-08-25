package com.dc.melodiasmario.feature.guild.data.repository.guild

import com.dc.melodiasmario.core.auth.data.session.AuthorizedSessionProvider
import com.dc.melodiasmario.feature.guild.data.remote.guild.GuildRemoteDataSource
import com.dc.melodiasmario.feature.guild.domain.model.guild.Guild
import com.dc.melodiasmario.feature.guild.domain.repository.guild.GuildRepository
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