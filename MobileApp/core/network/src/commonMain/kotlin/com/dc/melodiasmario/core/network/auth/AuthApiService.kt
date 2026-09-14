package com.dc.melodiasmario.core.network.auth

import com.dc.melodiasmario.core.network.auth.dto.AuthSessionDto
import com.dc.melodiasmario.core.network.auth.dto.DiscordLoginUrlDto
import com.dc.melodiasmario.core.network.auth.dto.ExchangeAuthTicketRequestDto
import com.dc.melodiasmario.core.network.auth.dto.RefreshSessionRequestDto
import com.dc.melodiasmario.core.common.AppConstants
import io.ktor.client.HttpClient
import io.ktor.client.call.body
import io.ktor.client.request.post
import io.ktor.client.request.setBody
import io.ktor.http.ContentType
import io.ktor.http.contentType
import org.koin.core.annotation.Property
import org.koin.core.annotation.Single

@Single
class AuthApiService(
    @Property(AppConstants.Properties.ApiBaseUrl)
    private val baseUrl: String,
    private val client: HttpClient,
) {
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
