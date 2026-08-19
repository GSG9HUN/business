package com.dc.melodiasmario.feature.guild.data.remote

import com.dc.melodiasmario.feature.guild.data.remote.dto.toDomain
import com.dc.melodiasmario.feature.guild.domain.model.Guild
import org.koin.core.annotation.Single

@Single(binds = [GuildRemoteDataSource::class])
class GuildRemoteDataSourceImpl(
    private val guildApiService: GuildApiService,
): GuildRemoteDataSource {
    override suspend fun getGuilds(accessToken: String): List<Guild> {
        return guildApiService.getGuilds(accessToken = accessToken).map { it.toDomain() }
    }
}
