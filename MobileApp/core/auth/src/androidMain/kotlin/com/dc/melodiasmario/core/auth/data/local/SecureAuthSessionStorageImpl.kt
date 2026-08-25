package com.dc.melodiasmario.core.auth.data.local

import com.dc.melodiasmario.core.auth.domain.model.AuthSession
import com.dc.melodiasmario.core.auth.domain.storage.SecureAuthSessionStorage
import org.koin.core.annotation.Single

//TODO expect actual használat -> majd átírni Keychain-re ios-en, EncryptedSharedPreferences vagy encrypted DataStore-ra androidon

@Single(binds = [SecureAuthSessionStorage::class])
class SecureAuthSessionStorageImpl: SecureAuthSessionStorage {
    private var session: AuthSession? = null
    override suspend fun saveSession(session: AuthSession){
        this.session = session
    }

    override suspend fun getSession(): AuthSession? {
        return session
    }

    override suspend fun clearSession() {
        session = null
    }
}