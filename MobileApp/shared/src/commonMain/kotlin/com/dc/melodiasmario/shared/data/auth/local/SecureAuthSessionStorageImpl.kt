package com.dc.melodiasmario.shared.data.auth.local

import com.dc.melodiasmario.shared.domain.auth.model.AuthSession
import com.dc.melodiasmario.shared.domain.auth.storage.SecureAuthSessionStorage
import org.koin.core.annotation.Singleton

//TODO majd átírni EncryptedSharedPreferences vagy encrypted DataStore-ra androidon
//TODO ios-en pedig keychain-re -> expect actual használat.

@Singleton
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