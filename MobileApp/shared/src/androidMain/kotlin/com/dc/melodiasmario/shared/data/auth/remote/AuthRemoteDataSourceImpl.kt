package com.dc.melodiasmario.shared.data.auth.remote

import com.dc.melodiasmario.shared.data.ApiService
import com.dc.melodiasmario.shared.data.auth.remote.dto.toDomain
import com.dc.melodiasmario.shared.domain.auth.model.AuthSession
import com.dc.melodiasmario.shared.domain.auth.model.DiscordLoginUrl
import org.koin.core.annotation.Single

@Single(binds = [AuthRemoteDataSource::class])
class AuthRemoteDataSourceImpl(
    private val apiService: ApiService
) : AuthRemoteDataSource {
    override suspend fun startDiscordLogin(): DiscordLoginUrl = apiService.startDiscordLogin().toDomain()

    override suspend fun exchangeTicket(ticket: String): AuthSession = apiService.exchangeTicket(ticket = ticket).toDomain()

    override suspend fun refreshSession(refreshToken: String): AuthSession = apiService.refreshSession(refreshToken = refreshToken).toDomain()

    override suspend fun logout(refreshToken: String) = apiService.logout(refreshToken = refreshToken)
}