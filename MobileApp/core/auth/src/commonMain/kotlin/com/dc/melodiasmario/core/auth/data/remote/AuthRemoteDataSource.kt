package com.dc.melodiasmario.core.auth.data.remote

import com.dc.melodiasmario.core.auth.domain.model.AuthSession
import com.dc.melodiasmario.core.auth.domain.model.DiscordLoginUrl

interface AuthRemoteDataSource {
    suspend fun startDiscordLogin(): DiscordLoginUrl
    suspend fun exchangeTicket(ticket: String): AuthSession
    suspend fun refreshSession(refreshToken: String): AuthSession
    suspend fun logout(refreshToken: String)
}