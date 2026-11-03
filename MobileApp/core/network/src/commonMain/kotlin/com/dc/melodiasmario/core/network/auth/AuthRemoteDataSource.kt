package com.dc.melodiasmario.core.network.auth

import com.dc.melodiasmario.core.model.auth.AuthSession
import com.dc.melodiasmario.core.model.auth.DiscordLoginUrl

/**
 * Defines remote auth data operations.
 */
interface AuthRemoteDataSource {
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
