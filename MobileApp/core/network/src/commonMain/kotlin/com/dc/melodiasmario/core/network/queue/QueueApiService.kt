package com.dc.melodiasmario.core.network.queue

import com.dc.melodiasmario.core.common.AppConstants
import com.dc.melodiasmario.core.network.botcontrol.dto.BotControlCommandDto
import com.dc.melodiasmario.core.network.queue.dto.EnqueueRequestDto
import com.dc.melodiasmario.core.network.queue.dto.QueueDto
import io.ktor.client.HttpClient
import io.ktor.client.call.body
import io.ktor.client.request.delete
import io.ktor.client.request.get
import io.ktor.client.request.header
import io.ktor.client.request.patch
import io.ktor.client.request.post
import io.ktor.client.request.setBody
import io.ktor.http.ContentType
import io.ktor.http.HttpHeaders
import org.koin.core.annotation.Property
import org.koin.core.annotation.Single

/**
 * Sends queue API requests through the configured HTTP client.
 */
@Single
class QueueApiService(
    @Property(value = AppConstants.Properties.ApiBaseUrl)
    private val baseUrl: String,
    private val httpClient: HttpClient
) {

    /**
     * Retrieves the current playback queue for the requested guild.
     */
    suspend fun getQueue(accessToken: String, guildId: String): QueueDto {
        return httpClient.get(urlString = "$baseUrl/guilds/$guildId/queue") {
            header(key = "Authorization", value = "Bearer $accessToken")
        }
            .body()
    }

    /**
     * Requests adding the supplied music query or URL to the guild playback queue.
     */
    suspend fun addToQueue(
        accessToken: String,
        guildId: String,
        query: String
    ): BotControlCommandDto {
        return httpClient.post(urlString = "$baseUrl/guilds/$guildId/queue/enqueue") {
            header(key = "Authorization", value = "Bearer $accessToken")
            header(key = HttpHeaders.ContentType, value = ContentType.Application.Json.toString())
            setBody(body = EnqueueRequestDto(query = query))
        }
            .body()
    }

    /**
     * Requests removal of a track from the playback queue.
     */
    suspend fun removeFromQueue(
        accessToken: String,
        guildId: String,
        trackNumber: String
    ): BotControlCommandDto {
        return httpClient.delete(urlString = "$baseUrl/guilds/$guildId/queue/$trackNumber") {
            header(key = "Authorization", value = "Bearer $accessToken")
        }
            .body()
    }

    /**
     * Requests clearing the guild playback queue.
     */
    suspend fun clearQueue(accessToken: String, guildId: String): BotControlCommandDto {
        return httpClient.delete(urlString = "$baseUrl/guilds/$guildId/queue") {
            header(key = "Authorization", value = "Bearer $accessToken")
        }
            .body()
    }

    /**
     * Requests shuffling the guild playback queue.
     */
    suspend fun shuffleQueue(accessToken: String, guildId: String): BotControlCommandDto {
        return httpClient.post(urlString = "$baseUrl/guilds/$guildId/queue/shuffle") {
            header(key = "Authorization", value = "Bearer $accessToken")
        }
            .body()
    }

    /**
     * Requests moving a queued track between zero-based queue indexes.
     */
    suspend fun moveTrackToIndex(
        accessToken: String,
        guildId: String,
        trackIndex: Int,
        targetIndex: Int
    ): BotControlCommandDto {
        return httpClient.patch(urlString = "$baseUrl/guilds/$guildId/queue/$trackIndex/move-to/$targetIndex") {
            header(key = "Authorization", value = "Bearer $accessToken")
        }
            .body()
    }
}
