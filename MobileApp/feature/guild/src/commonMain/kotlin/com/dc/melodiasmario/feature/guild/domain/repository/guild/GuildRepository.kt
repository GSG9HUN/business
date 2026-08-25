package com.dc.melodiasmario.feature.guild.domain.repository.guild

import com.dc.melodiasmario.feature.guild.domain.model.guild.Guild

interface GuildRepository {
    suspend fun getGuilds(): List<Guild>
}