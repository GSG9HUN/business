package com.dc.melodiasmario.shared.domain.auth.repository

import com.dc.melodiasmario.shared.domain.auth.model.AuthSession
import com.dc.melodiasmario.shared.domain.auth.model.DiscordLoginUrl

interface AuthRepository {
    suspend fun startDiscordLogin(): DiscordLoginUrl
    suspend fun exchangeTicket(ticket: String): AuthSession
    suspend fun refreshSession(refreshToken: String): AuthSession
    suspend fun logout(refreshToken: String)
}