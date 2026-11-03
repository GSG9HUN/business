package com.dc.melodiasmario.core.network.guild

import com.dc.melodiasmario.core.common.AppConstants
import com.dc.melodiasmario.core.network.guild.dto.GuildDto
import io.ktor.client.HttpClient
import io.ktor.client.call.body
import io.ktor.client.request.get
import io.ktor.client.request.header
import org.koin.core.annotation.Property
import org.koin.core.annotation.Single

/**
 * Sends guild API requests through the configured HTTP client.
 */
@Single
class GuildApiService(
    @Property(value = AppConstants.Properties.ApiBaseUrl)
    private val baseUrl: String,
    private val httpClient: HttpClient
) {
    /**
     * Retrieves the guilds available to the authenticated user.
     */
    suspend fun getGuilds(accessToken: String): List<GuildDto> {
        return httpClient.get(urlString = "$baseUrl/guilds/") {
            header(key = "Authorization", value = "Bearer $accessToken")
        }
            .body()
    }

    /**
     * Retrieves details for the selected guild.
     */
    suspend fun getSelectedGuild(accessToken: String, guildId: String): GuildDto? {
        return httpClient.get(urlString = "$baseUrl/guilds/$guildId") {
            header(key = "Authorization", value = "Bearer $accessToken")
        }
            .body<GuildDto?>()
    }
}
