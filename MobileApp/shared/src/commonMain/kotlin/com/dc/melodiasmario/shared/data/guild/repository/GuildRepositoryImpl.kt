package com.dc.melodiasmario.shared.data.guild.repository

import com.dc.melodiasmario.shared.data.auth.session.AuthorizedSessionProvider
import com.dc.melodiasmario.shared.data.guild.remote.GuildRemoteDataSource
import com.dc.melodiasmario.shared.domain.guild.model.Guild
import com.dc.melodiasmario.shared.domain.guild.repository.GuildRepository
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
