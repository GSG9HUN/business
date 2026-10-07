package com.dc.melodiasmario.core.network.search

import com.dc.melodiasmario.core.common.AppConstants
import com.dc.melodiasmario.core.network.search.dto.MusicSearchCapabilityDto
import com.dc.melodiasmario.core.network.search.dto.MusicSearchResponseDto
import io.ktor.client.HttpClient
import io.ktor.client.call.body
import io.ktor.client.request.get
import io.ktor.client.request.header
import io.ktor.client.request.parameter
import org.koin.core.annotation.Single
import org.koin.core.annotation.Property

@Single
class MusicSearchApiService(
    private val client: HttpClient,
    @Property(AppConstants.Properties.ApiBaseUrl)
    private val baseUrl: String,
) {
    suspend fun getCapabilities(
        accessToken: String,
        guildId: String,
    ): List<MusicSearchCapabilityDto> {
        return client.get("$baseUrl/guilds/$guildId/music-search/capabilities") {
            header("Authorization", "Bearer $accessToken")
        }.body()
    }

    suspend fun search(
        accessToken: String,
        guildId: String,
        provider: String,
        kind: String,
        query: String,
        pageToken: String?,
    ): MusicSearchResponseDto {
        return client.get("$baseUrl/guilds/$guildId/music-search") {
            header("Authorization", "Bearer $accessToken")
            parameter("provider", provider)
            parameter("kind", kind)
            parameter("query", query)
            pageToken?.let { parameter("pageToken", it) }
        }.body()
    }

}
