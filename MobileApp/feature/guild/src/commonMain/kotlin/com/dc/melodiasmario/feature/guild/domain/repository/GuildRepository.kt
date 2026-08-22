package com.dc.melodiasmario.feature.guild.domain.repository

import com.dc.melodiasmario.feature.guild.domain.model.Guild

interface GuildRepository {
    suspend fun getGuilds(): List<Guild>
}
