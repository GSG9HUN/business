package com.dc.melodiasmario.shared.data.auth.repository

import com.dc.melodiasmario.shared.data.auth.remote.AuthRemoteDataSource
import com.dc.melodiasmario.shared.domain.auth.model.AuthSession
import com.dc.melodiasmario.shared.domain.auth.storage.SecureAuthSessionStorage
import com.dc.melodiasmario.shared.domain.auth.model.DiscordLoginUrl
import com.dc.melodiasmario.shared.domain.auth.repository.AuthRepository
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