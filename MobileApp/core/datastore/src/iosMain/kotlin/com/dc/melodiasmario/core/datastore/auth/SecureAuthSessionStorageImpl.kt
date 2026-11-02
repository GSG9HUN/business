package com.dc.melodiasmario.core.datastore.auth

import com.dc.melodiasmario.core.domain.auth.SecureAuthSessionStorage
import com.dc.melodiasmario.core.model.auth.AuthSession
import org.koin.core.annotation.Single

/**
 * Provides secure auth session storage impl for the auth layer.
 */
//TODO expect actual használat -> majd átvinni Keychain-re ios-en, EncryptedSharedPreferences vagy encrypted DataStore-ra androidon
@Single(binds = [SecureAuthSessionStorage::class])
class SecureAuthSessionStorageImpl : SecureAuthSessionStorage {
    private var session: AuthSession? = null
    override suspend fun saveSession(session: AuthSession): Unit{
        this.session = session
    }

    override suspend fun getSession(): AuthSession? {
        return session
    }

    override suspend fun clearSession(): Unit {
        session = null
    }
}
