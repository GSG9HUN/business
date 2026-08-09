package com.dc.melodiasmario.shared.data

import com.dc.melodiasmario.shared.AppConstants
import com.dc.melodiasmario.shared.data.auth.remote.dto.AuthSessionDto
import com.dc.melodiasmario.shared.data.auth.remote.dto.DiscordLoginUrlDto
import com.dc.melodiasmario.shared.data.auth.remote.dto.ExchangeAuthTicketRequestDto
import com.dc.melodiasmario.shared.data.auth.remote.dto.RefreshSessionRequestDto
import com.dc.melodiasmario.shared.data.guild.remote.dto.GuildDto
import io.ktor.client.HttpClient
import io.ktor.client.call.body
import io.ktor.client.request.get
import io.ktor.client.request.header
import io.ktor.client.request.post
import io.ktor.client.request.setBody
import io.ktor.http.ContentType
import io.ktor.http.contentType
import org.koin.core.annotation.Property
import org.koin.core.annotation.Single

@Single
class ApiService(
    @Property(AppConstants.Properties.ApiBaseUrl)
    private val baseUrl: String,
    private val client: HttpClient,
) {
    suspend fun getGuilds(accessToken: String): List<GuildDto> {
        return client.get("$baseUrl/guilds/") {
            header("Authorization", "Bearer $accessToken")
        }.body()
    }

    suspend fun isApiOnline(): Boolean {
        return client.get("$baseUrl/status").status.value in 200..299
    }

    suspend fun startDiscordLogin(): DiscordLoginUrlDto {
        return client.post("$baseUrl/auth/discord/start").body()
    }

    suspend fun exchangeTicket(ticket: String): AuthSessionDto {
        return client.post("$baseUrl/auth/exchange"){
            contentType(ContentType.Application.Json)
            setBody(ExchangeAuthTicketRequestDto(ticket = ticket))
        }.body()
    }

    suspend fun refreshSession(refreshToken: String): AuthSessionDto {
        return client.post("$baseUrl/auth/refresh") {
            contentType(ContentType.Application.Json)
            setBody(RefreshSessionRequestDto(refreshToken = refreshToken))
        }.body()
    }

    suspend fun logout(refreshToken: String) {
        return client.post("$baseUrl/auth/logout"){
            contentType(ContentType.Application.Json)
            setBody(RefreshSessionRequestDto(refreshToken = refreshToken))
        }.body()
    }
}
