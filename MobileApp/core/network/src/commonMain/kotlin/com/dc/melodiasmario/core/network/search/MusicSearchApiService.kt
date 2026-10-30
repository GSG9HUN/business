package com.dc.melodiasmario.core.network.search

import com.dc.melodiasmario.core.common.AppConstants
import com.dc.melodiasmario.core.network.search.dto.MusicSearchCapabilityDto
import com.dc.melodiasmario.core.network.search.dto.MusicSearchResponseDto
import io.ktor.client.HttpClient
import io.ktor.client.call.body
import io.ktor.client.request.get
import io.ktor.client.request.header
import io.ktor.client.request.parameter
import org.koin.core.annotation.Property
import org.koin.core.annotation.Single

/**
 * Sends music search API requests through the configured HTTP client.
 */
@Single
class MusicSearchApiService(
    @Property(value = AppConstants.Properties.ApiBaseUrl)
    private val baseUrl: String,
    private val httpClient: HttpClient
) {
    /**
     * Retrieves the music-search capabilities available for the selected guild.
     */
    suspend fun getCapabilities(
        accessToken: String,
        guildId: String
    ): List<MusicSearchCapabilityDto> {
        return httpClient.get(urlString = "$baseUrl/guilds/$guildId/music-search/capabilities") {
            header(key = "Authorization", value = "Bearer $accessToken")
        }
            .body()
    }

    /**
     * Searches for music using the selected provider, result kind, query, and continuation token.
     */
    suspend fun search(
        accessToken: String,
        guildId: String,
        provider: String,
        kind: String,
        query: String,
        pageToken: String?
    ): MusicSearchResponseDto {
        return httpClient.get(urlString = "$baseUrl/guilds/$guildId/music-search") {
            header(key = "Authorization", value = "Bearer $accessToken")
            parameter(key = "provider", value = provider)
            parameter(key = "kind", value = kind)
            parameter(key = "query", value = query)
            pageToken?.let { pageToken -> parameter(key = "pageToken", value = pageToken) }
        }
            .body()
    }

}
