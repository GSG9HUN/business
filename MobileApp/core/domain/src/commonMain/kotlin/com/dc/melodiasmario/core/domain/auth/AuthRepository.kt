package com.dc.melodiasmario.core.domain.auth

import com.dc.melodiasmario.core.model.auth.AuthSession
import com.dc.melodiasmario.core.model.auth.DiscordLoginUrl

/**
 * Defines auth data operations independently of their concrete data sources.
 */
interface AuthRepository {
    /**
     * Starts the Discord authentication flow and returns its authorization information.
     */
    suspend fun startDiscordLogin(): DiscordLoginUrl

    /**
     * Exchanges a one-time authentication ticket for a session.
     */
    suspend fun exchangeTicket(ticket: String): AuthSession

    /**
     * Refreshes the authentication session using its refresh token.
     */
    suspend fun refreshSession(refreshToken: String): AuthSession

    /**
     * Ends the authentication session associated with the refresh token.
     */
    suspend fun logout(refreshToken: String): Unit
}
