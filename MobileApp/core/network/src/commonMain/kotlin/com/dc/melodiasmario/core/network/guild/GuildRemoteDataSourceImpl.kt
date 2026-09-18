package com.dc.melodiasmario.core.network.guild

import com.dc.melodiasmario.core.model.guild.Guild
import com.dc.melodiasmario.core.network.guild.dto.GuildDto
import com.dc.melodiasmario.core.network.guild.dto.toDomain
import org.koin.core.annotation.Single

@Single(binds = [GuildRemoteDataSource::class])
class GuildRemoteDataSourceImpl(
    private val guildApiService: GuildApiService,
) : GuildRemoteDataSource {
    override suspend fun getGuilds(accessToken: String): List<Guild> {
        return guildApiService.getGuilds(accessToken = accessToken).map { it.toDomain() }
    }

    override suspend fun getSelectedGuild(
        accessToken: String,
        guildId: String
    ): Guild? {
        return guildApiService.getSelectedGuild(accessToken = accessToken, guildId = guildId)?.toDomain()
    }
}
