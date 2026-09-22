package com.dc.melodiasmario.core.network.currenttrack

import com.dc.melodiasmario.core.common.AppConstants
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

@Single
class CurrentTrackApiService(
    @Property(AppConstants.Properties.ApiBaseUrl)
    private val baseUrl: String,
    private val client: HttpClient,
) {
    suspend fun getPlaybackStatus(accessToken: String, guildId: String): PlaybackStatusDto {
        return client.get("$baseUrl/guilds/$guildId/player") {
            header("Authorization", "Bearer $accessToken")
        }.body<PlaybackStatusDto>()
    }

    suspend fun nextTrack(accessToken: String, guildId: String) {
        return client.post("$baseUrl/guilds/$guildId/playback/skip") {
            header("Authorization", "Bearer $accessToken")
        }.body()
    }

    suspend fun previousTrack(accessToken: String, guildId: String) {
        return client.post("$baseUrl/guilds/$guildId/playback/previous") {
            header("Authorization", "Bearer $accessToken")
        }.body()
    }

    suspend fun play(accessToken: String, guildId: String) {
        return client.post("$baseUrl/guilds/$guildId/playback/resume") {
            header("Authorization", "Bearer $accessToken")
        }.body()
    }

    suspend fun pause(accessToken: String, guildId: String) {
        return client.post("$baseUrl/guilds/$guildId/playback/pause") {
            header("Authorization", "Bearer $accessToken")
        }.body()
    }

    suspend fun setRepeatMode(accessToken: String, guildId: String, mode: String) {
        return client.patch("$baseUrl/guilds/$guildId/playback/repeat-mode") {
            header("Authorization", "Bearer $accessToken")
            header(HttpHeaders.ContentType, ContentType.Application.Json.toString())
            setBody(SetRepeatModeRequestDto(mode = mode))
        }.body()
    }
}
