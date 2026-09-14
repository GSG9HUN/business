package com.dc.melodiasmario.core.domain.auth

import com.dc.melodiasmario.core.model.auth.AuthSession
import com.dc.melodiasmario.core.model.auth.DiscordLoginUrl

interface AuthRepository {
    suspend fun startDiscordLogin(): DiscordLoginUrl
    suspend fun exchangeTicket(ticket: String): AuthSession
    suspend fun refreshSession(refreshToken: String): AuthSession
    suspend fun logout(refreshToken: String)
}
