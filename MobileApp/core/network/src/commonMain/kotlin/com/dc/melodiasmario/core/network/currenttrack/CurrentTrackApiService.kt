package com.dc.melodiasmario.core.network.currenttrack

import com.dc.melodiasmario.core.common.AppConstants
import com.dc.melodiasmario.core.network.botcontrol.dto.BotControlCommandDto
import com.dc.melodiasmario.core.network.currenttrack.dto.PlaybackStatusDto
import com.dc.melodiasmario.core.network.currenttrack.dto.SetRepeatModeRequestDto
import io.ktor.client.HttpClient
import io.ktor.client.call.body
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
 * Sends current track API requests through the configured HTTP client.
 */
@Single
class CurrentTrackApiService(
    @Property(value = AppConstants.Properties.ApiBaseUrl)
    private val baseUrl: String,
    private val httpClient: HttpClient
) {
    /**
     * Retrieves the guild playback status and current track.
     */
    suspend fun getPlaybackStatus(accessToken: String, guildId: String): PlaybackStatusDto {
        return httpClient.get(urlString = "$baseUrl/guilds/$guildId/player") {
            header(key = "Authorization", value = "Bearer $accessToken")
        }
            .body<PlaybackStatusDto>()
    }

    /**
     * Requests advancing to the next queued track.
     */
    suspend fun nextTrack(accessToken: String, guildId: String): BotControlCommandDto {
        return httpClient.post(urlString = "$baseUrl/guilds/$guildId/playback/skip") {
            header(key = "Authorization", value = "Bearer $accessToken")
        }
            .body()
    }

    /**
     * Requests playing the previous track.
     */
    suspend fun previousTrack(accessToken: String, guildId: String): BotControlCommandDto {
        return httpClient.post(urlString = "$baseUrl/guilds/$guildId/playback/previous") {
            header(key = "Authorization", value = "Bearer $accessToken")
        }
            .body()
    }

    /**
     * Requests resuming guild playback.
     */
    suspend fun play(accessToken: String, guildId: String): BotControlCommandDto {
        return httpClient.post(urlString = "$baseUrl/guilds/$guildId/playback/resume") {
            header(key = "Authorization", value = "Bearer $accessToken")
        }
            .body()
    }

    /**
     * Requests pausing guild playback.
     */
    suspend fun pause(accessToken: String, guildId: String): BotControlCommandDto {
        return httpClient.post(urlString = "$baseUrl/guilds/$guildId/playback/pause") {
            header(key = "Authorization", value = "Bearer $accessToken")
        }
            .body()
    }

    /**
     * Requests updating the playback repeat mode.
     */
    suspend fun setRepeatMode(accessToken: String, guildId: String, mode: String): Unit {
        return httpClient.patch(urlString = "$baseUrl/guilds/$guildId/playback/repeat-mode") {
            header(key = "Authorization", value = "Bearer $accessToken")
            header(key = HttpHeaders.ContentType, value = ContentType.Application.Json.toString())
            setBody(body = SetRepeatModeRequestDto(mode = mode))
        }
            .body()
    }
}
