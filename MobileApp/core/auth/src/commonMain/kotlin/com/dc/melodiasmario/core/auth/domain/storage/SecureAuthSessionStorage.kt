package com.dc.melodiasmario.core.auth.domain.storage

import com.dc.melodiasmario.core.auth.domain.model.AuthSession

interface SecureAuthSessionStorage {
    suspend fun saveSession(session: AuthSession)
    suspend fun clearSession()
    suspend fun getSession(): AuthSession?
}