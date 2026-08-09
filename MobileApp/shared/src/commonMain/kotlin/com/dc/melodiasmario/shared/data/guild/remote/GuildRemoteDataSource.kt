package com.dc.melodiasmario.shared.data.guild.remote

import com.dc.melodiasmario.shared.domain.guild.model.Guild

interface GuildRemoteDataSource {
    suspend fun getGuilds(accessToken: String): List<Guild>
}
