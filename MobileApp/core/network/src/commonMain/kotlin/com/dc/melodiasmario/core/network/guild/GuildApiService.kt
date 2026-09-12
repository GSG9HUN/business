package com.dc.melodiasmario.core.network.guild

import com.dc.melodiasmario.core.common.AppConstants
import com.dc.melodiasmario.core.network.guild.dto.GuildDto
import io.ktor.client.HttpClient
import io.ktor.client.call.body
import io.ktor.client.request.get
import io.ktor.client.request.header
import org.koin.core.annotation.Property
import org.koin.core.annotation.Single

@Single
class GuildApiService(
    @Property(AppConstants.Properties.ApiBaseUrl)
    private val baseUrl: String,
    private val client: HttpClient,
) {
    suspend fun getGuilds(accessToken: String): List<GuildDto> {
        return client.get("$baseUrl/guilds/") {
            header("Authorization", "Bearer $accessToken")
        }.body()
    }
}