package com.dc.melodiasmario.core.domain.guild

import com.dc.melodiasmario.core.model.guild.Guild

/**
 * Defines guild data operations independently of their concrete data sources.
 */
interface GuildRepository {
    /**
     * Retrieves the guilds available to the authenticated user.
     */
    suspend fun getGuilds(): List<Guild>

    /**
     * Retrieves details for the selected guild.
     */
    suspend fun getSelectedGuild(guildId: String): Guild?
}
