package com.dc.melodiasmario.shared.domain.auth.storage

import com.dc.melodiasmario.shared.domain.auth.model.AuthSession

interface SecureAuthSessionStorage {
    suspend fun saveSession(session: AuthSession)
    suspend fun clearSession()
    suspend fun getSession(): AuthSession?
}