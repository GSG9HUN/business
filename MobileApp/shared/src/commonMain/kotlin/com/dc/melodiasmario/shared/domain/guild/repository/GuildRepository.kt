package com.dc.melodiasmario.shared.domain.guild.repository

import com.dc.melodiasmario.shared.domain.guild.model.Guild

interface GuildRepository {
    suspend fun getGuilds(): List<Guild>
}
