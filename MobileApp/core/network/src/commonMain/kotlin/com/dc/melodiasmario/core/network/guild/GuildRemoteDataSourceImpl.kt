package com.dc.melodiasmario.core.network.guild

import com.dc.melodiasmario.core.model.guild.Guild
import org.koin.core.annotation.Single

/**
 * Adapts remote guild responses to domain-facing data.
 */
@Single(binds = [GuildRemoteDataSource::class])
class GuildRemoteDataSourceImpl(
    private val guildApiService: GuildApiService
) : GuildRemoteDataSource {
    override suspend fun getGuilds(accessToken: String): List<Guild> {
        return guildApiService.getGuilds(accessToken = accessToken)
            .map { guild -> guild.toDomain() }
    }

    override suspend fun getSelectedGuild(
        accessToken: String,
        guildId: String
    ): Guild? {
        return guildApiService.getSelectedGuild(accessToken = accessToken, guildId = guildId)
            ?.toDomain()
    }
}
