package com.dc.melodiasmario.core.network.audioeffect

import com.dc.melodiasmario.core.common.AppConstants
import com.dc.melodiasmario.core.network.audioeffect.dto.AudioEffectsDto
import com.dc.melodiasmario.core.network.audioeffect.dto.AudioEffectsRequestDto
import com.dc.melodiasmario.core.network.audioeffect.dto.SaveAudioEffectPresetRequestDto
import com.dc.melodiasmario.core.network.botcontrol.dto.BotControlCommandDto
import io.ktor.client.HttpClient
import io.ktor.client.call.body
import io.ktor.client.request.get
import io.ktor.client.request.header
import io.ktor.client.request.post
import io.ktor.client.request.setBody
import io.ktor.http.ContentType
import io.ktor.http.HttpHeaders
import org.koin.core.annotation.Property
import org.koin.core.annotation.Single

/** Sends guild audio-effect and preset requests through the configured HTTP client. */
@Single
class AudioEffectApiService(
    @Property(value = AppConstants.Properties.ApiBaseUrl)
    private val baseUrl: String,
    private val httpClient: HttpClient
) {
    /** Reads the last confirmed filter snapshot and one page of guild presets. */
    suspend fun getAudioEffects(accessToken: String, guildId: String, page: Int): AudioEffectsDto {
        return httpClient.get(urlString = "$baseUrl/guilds/$guildId/playback/audio-effects?page=$page") {
            header(key = HttpHeaders.Authorization, value = "Bearer $accessToken")
        }
            .body()
    }

    /** Accepts a bounded filter batch; terminal completion is observed separately. */
    suspend fun applyAudioEffects(accessToken: String, guildId: String, commands: List<String>): BotControlCommandDto {
        return httpClient.post(urlString = "$baseUrl/guilds/$guildId/playback/audio-effects") {
            header(key = HttpHeaders.Authorization, value = "Bearer $accessToken")
            header(key = HttpHeaders.ContentType, value = ContentType.Application.Json.toString())
            setBody(body = AudioEffectsRequestDto(commands = commands))
        }
            .body()
    }

    /** Requests preset persistence only; draft settings are not applied to the live player. */
    suspend fun saveAudioEffectPreset(
        accessToken: String,
        guildId: String,
        name: String,
        commands: List<String>,
        overwrite: Boolean
    ): BotControlCommandDto {
        return httpClient.post(urlString = "$baseUrl/guilds/$guildId/playback/audio-effects/presets") {
            header(key = HttpHeaders.Authorization, value = "Bearer $accessToken")
            header(key = HttpHeaders.ContentType, value = ContentType.Application.Json.toString())
            setBody(body = SaveAudioEffectPresetRequestDto(name = name, commands = commands, overwrite = overwrite))
        }
            .body()
    }
}
