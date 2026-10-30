package com.dc.melodiasmario.core.network.auth

import com.dc.melodiasmario.core.common.AppConstants
import com.dc.melodiasmario.core.network.auth.dto.AuthSessionDto
import com.dc.melodiasmario.core.network.auth.dto.DiscordLoginUrlDto
import com.dc.melodiasmario.core.network.auth.dto.ExchangeAuthTicketRequestDto
import com.dc.melodiasmario.core.network.auth.dto.RefreshSessionRequestDto
import io.ktor.client.HttpClient
import io.ktor.client.call.body
import io.ktor.client.request.post
import io.ktor.client.request.setBody
import io.ktor.http.ContentType
import io.ktor.http.contentType
import org.koin.core.annotation.Property
import org.koin.core.annotation.Single

/**
 * Sends auth API requests through the configured HTTP client.
 */
@Single
class AuthApiService(
    @Property(value = AppConstants.Properties.ApiBaseUrl)
    private val baseUrl: String,
    private val httpClient: HttpClient
) {
    /**
     * Starts the Discord authentication flow and returns its authorization information.
     */
    suspend fun startDiscordLogin(): DiscordLoginUrlDto {
        return httpClient.post(urlString = "$baseUrl/auth/discord/start")
            .body()
    }

    /**
     * Exchanges a one-time authentication ticket for a session.
     */
    suspend fun exchangeTicket(ticket: String): AuthSessionDto {
        return httpClient.post(urlString = "$baseUrl/auth/exchange"){
            contentType(type = ContentType.Application.Json)
            setBody(body = ExchangeAuthTicketRequestDto(ticket = ticket))
        }
            .body()
    }

    /**
     * Refreshes the authentication session using its refresh token.
     */
    suspend fun refreshSession(refreshToken: String): AuthSessionDto {
        return httpClient.post(urlString = "$baseUrl/auth/refresh") {
            contentType(type = ContentType.Application.Json)
            setBody(body = RefreshSessionRequestDto(refreshToken = refreshToken))
        }
            .body()
    }

    /**
     * Ends the authentication session associated with the refresh token.
     */
    suspend fun logout(refreshToken: String): Unit {
        return httpClient.post(urlString = "$baseUrl/auth/logout"){
            contentType(type = ContentType.Application.Json)
            setBody(body = RefreshSessionRequestDto(refreshToken = refreshToken))
        }
            .body()
    }
}
