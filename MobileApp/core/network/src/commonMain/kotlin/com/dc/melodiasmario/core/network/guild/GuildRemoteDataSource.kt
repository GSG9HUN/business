package com.dc.melodiasmario.core.network.guild

import com.dc.melodiasmario.core.model.guild.Guild

interface GuildRemoteDataSource {
    suspend fun getGuilds(accessToken: String): List<Guild>
}
