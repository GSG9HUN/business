package com.dc.melodiasmario.core.data.auth

import com.dc.melodiasmario.core.network.auth.AuthRemoteDataSource
import com.dc.melodiasmario.core.model.auth.AuthSession
import com.dc.melodiasmario.core.model.auth.DiscordLoginUrl
import com.dc.melodiasmario.core.domain.auth.AuthRepository
import com.dc.melodiasmario.core.domain.auth.SecureAuthSessionStorage

import org.koin.core.annotation.Single

@Single(binds = [AuthRepository::class])
class AuthRepositoryImpl (
    private val authRemoteDataSource: AuthRemoteDataSource,
    private val secureAuthSessionStorage: SecureAuthSessionStorage
): AuthRepository {
    override suspend fun startDiscordLogin(): DiscordLoginUrl = authRemoteDataSource.startDiscordLogin()

    override suspend fun exchangeTicket(ticket: String): AuthSession {
        val session = authRemoteDataSource.exchangeTicket(ticket = ticket)
        secureAuthSessionStorage.saveSession(session)
        return session
    }

    override suspend fun refreshSession(refreshToken: String): AuthSession {
        val session = authRemoteDataSource.refreshSession(refreshToken = refreshToken)
        secureAuthSessionStorage.saveSession(session)
        return session
    }

    override suspend fun logout(refreshToken: String) {
        authRemoteDataSource.logout(refreshToken = refreshToken)
        secureAuthSessionStorage.clearSession()
    }
}
