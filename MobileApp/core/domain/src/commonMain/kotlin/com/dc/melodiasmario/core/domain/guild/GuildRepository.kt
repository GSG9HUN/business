package com.dc.melodiasmario.core.domain.guild

import com.dc.melodiasmario.core.model.guild.Guild

interface GuildRepository {
    suspend fun getGuilds(): List<Guild>
}