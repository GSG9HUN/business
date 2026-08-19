package com.dc.melodiasmario.feature.guild.data.remote

import com.dc.melodiasmario.feature.guild.domain.model.Guild

interface GuildRemoteDataSource {
    suspend fun getGuilds(accessToken: String): List<Guild>
}
