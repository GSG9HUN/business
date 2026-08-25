package com.dc.melodiasmario.feature.guild.data.remote.guild

import com.dc.melodiasmario.feature.guild.domain.model.guild.Guild

interface GuildRemoteDataSource {
    suspend fun getGuilds(accessToken: String): List<Guild>
}
