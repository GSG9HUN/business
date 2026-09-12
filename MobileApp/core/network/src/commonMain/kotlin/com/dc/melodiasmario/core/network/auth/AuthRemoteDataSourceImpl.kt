package com.dc.melodiasmario.core.network.auth

import com.dc.melodiasmario.core.network.auth.dto.toDomain
import com.dc.melodiasmario.core.model.auth.AuthSession
import com.dc.melodiasmario.core.model.auth.DiscordLoginUrl
import org.koin.core.annotation.Single

@Single(binds = [AuthRemoteDataSource::class])
class AuthRemoteDataSourceImpl(
    private val authApiService: AuthApiService
) : AuthRemoteDataSource {
    override suspend fun startDiscordLogin(): DiscordLoginUrl = authApiService.startDiscordLogin().toDomain()

    override suspend fun exchangeTicket(ticket: String): AuthSession = authApiService.exchangeTicket(ticket = ticket).toDomain()

    override suspend fun refreshSession(refreshToken: String): AuthSession = authApiService.refreshSession(refreshToken = refreshToken).toDomain()

    override suspend fun logout(refreshToken: String) = authApiService.logout(refreshToken = refreshToken)
}
