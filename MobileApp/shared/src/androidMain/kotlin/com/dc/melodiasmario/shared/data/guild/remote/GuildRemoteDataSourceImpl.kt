package com.dc.melodiasmario.shared.data.guild.remote

import com.dc.melodiasmario.shared.data.ApiService
import com.dc.melodiasmario.shared.data.guild.remote.dto.toDomain
import com.dc.melodiasmario.shared.domain.guild.model.Guild
import org.koin.core.annotation.Single

@Single(binds = [GuildRemoteDataSource::class])
class GuildRemoteDataSourceImpl(
    private val apiService: ApiService,
): GuildRemoteDataSource {
    override suspend fun getGuilds(accessToken: String): List<Guild> {
        return apiService.getGuilds(accessToken = accessToken).map { it.toDomain() }
    }
}
