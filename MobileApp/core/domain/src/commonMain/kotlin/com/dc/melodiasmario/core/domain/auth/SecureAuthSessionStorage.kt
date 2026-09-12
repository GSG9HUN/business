package com.dc.melodiasmario.core.domain.auth

import com.dc.melodiasmario.core.model.auth.AuthSession

interface SecureAuthSessionStorage {
    suspend fun saveSession(session: AuthSession)
    suspend fun clearSession()
    suspend fun getSession(): AuthSession?
}
