package com.dc.melodiasmario.core.network.queue

import com.dc.melodiasmario.core.common.AppConstants
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

@Single
class QueueApiService(
    @Property(AppConstants.Properties.ApiBaseUrl)
    private val baseUrl: String,
    private val client: HttpClient,
) {

    suspend fun getQueue(accessToken: String, guildId: String): QueueDto {
        return client.get("$baseUrl/guilds/$guildId/queue") {
            header("Authorization", "Bearer $accessToken")
        }.body()
    }

    suspend fun addToQueue(accessToken: String, guildId: String, query: String) {
        return client.post("$baseUrl/guilds/$guildId/queue/enqueue") {
            header("Authorization", "Bearer $accessToken")
            header(HttpHeaders.ContentType, ContentType.Application.Json.toString())
            setBody(EnqueueRequestDto(query = query))
        }.body()
    }

    suspend fun removeFromQueue(accessToken: String, guildId: String, trackNumber: String) {
        return client.delete("$baseUrl/guilds/$guildId/queue/$trackNumber") {
            header("Authorization", "Bearer $accessToken")
        }.body()
    }

    suspend fun clearQueue(accessToken: String, guildId: String) {
        return client.delete("$baseUrl/guilds/$guildId/queue") {
            header("Authorization", "Bearer $accessToken")
        }.body()
    }

    suspend fun shuffleQueue(accessToken: String, guildId: String) {
        return client.post("$baseUrl/guilds/$guildId/queue/shuffle") {
            header("Authorization", "Bearer $accessToken")
        }.body()
    }

    suspend fun moveTrackUp(accessToken: String, guildId: String, trackIndex: Int) {
        return client.patch("$baseUrl/guilds/$guildId/queue/$trackIndex/move-up") {
            header("Authorization", "Bearer $accessToken")
        }.body()
    }

    suspend fun moveTrackDown(accessToken: String, guildId: String, trackIndex: Int) {
        return client.patch("$baseUrl/guilds/$guildId/queue/$trackIndex/move-down") {
            header("Authorization", "Bearer $accessToken")
        }.body()
    }
}
