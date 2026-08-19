package com.dc.melodiasmario.core.auth.data.repository

import com.dc.melodiasmario.core.auth.data.remote.AuthRemoteDataSource
import com.dc.melodiasmario.core.auth.domain.model.AuthSession
import com.dc.melodiasmario.core.auth.domain.model.DiscordLoginUrl
import com.dc.melodiasmario.core.auth.domain.repository.AuthRepository
import com.dc.melodiasmario.core.auth.domain.storage.SecureAuthSessionStorage

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