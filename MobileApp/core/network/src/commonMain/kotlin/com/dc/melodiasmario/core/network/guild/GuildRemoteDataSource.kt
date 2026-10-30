package com.dc.melodiasmario.core.network.guild

import com.dc.melodiasmario.core.model.guild.Guild

/**
 * Defines remote guild data operations.
 */
interface GuildRemoteDataSource {
    /**
     * Retrieves the guilds available to the authenticated user.
     */
    suspend fun getGuilds(accessToken: String): List<Guild>

    /**
     * Retrieves details for the selected guild.
     */
    suspend fun getSelectedGuild(accessToken: String, guildId: String): Guild?
}
