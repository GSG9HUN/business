package com.dc.melodiasmario.core.auth.data.remote

import com.dc.melodiasmario.core.auth.data.remote.dto.toDomain
import com.dc.melodiasmario.core.auth.domain.model.AuthSession
import com.dc.melodiasmario.core.auth.domain.model.DiscordLoginUrl
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
