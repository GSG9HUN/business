package com.dc.melodiasmario.core.domain.auth

import com.dc.melodiasmario.core.model.auth.AuthSession

/**
 * Provides secure auth session storage for the auth layer.
 */
interface SecureAuthSessionStorage {
    /**
     * Stores the authentication session for later restoration.
     */
    suspend fun saveSession(session: AuthSession): Unit

    /**
     * Removes the locally stored authentication session.
     */
    suspend fun clearSession(): Unit

    /**
     * Reads the locally stored authentication session.
     */
    suspend fun getSession(): AuthSession?
}
