package com.dc.melodiasmario.core.datastore.auth

import android.content.Context
import androidx.datastore.core.DataStore
import androidx.datastore.preferences.core.Preferences
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.core.stringPreferencesKey
import androidx.datastore.preferences.preferencesDataStore
import com.dc.melodiasmario.core.domain.auth.SecureAuthSessionStorage
import com.dc.melodiasmario.core.model.auth.AuthSession
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.withContext
import kotlinx.serialization.decodeFromString
import kotlinx.serialization.encodeToString
import kotlinx.serialization.json.Json
import org.koin.core.annotation.Single

private val Context.authSessionDataStore by preferencesDataStore(
    name = "secure_auth_session",
)

private val authSessionJson = Json {
    ignoreUnknownKeys = true
}

@Single(binds = [SecureAuthSessionStorage::class])
class SecureAuthSessionStorageImpl(
    private val context: Context,
) : SecureAuthSessionStorage {
    private var cipher: AuthSessionCipher = AuthSessionCipher()
    private var dataStore: DataStore<Preferences> = context.authSessionDataStore

    internal constructor(
        context: Context,
        cipher: AuthSessionCipher,
        dataStore: DataStore<Preferences>,
    ) : this(context) {
        this.cipher = cipher
        this.dataStore = dataStore
    }

    override suspend fun saveSession(session: AuthSession): Unit = withContext(Dispatchers.IO) {
        val encryptedSession = cipher.encrypt(session.toJson())

        dataStore.edit { preferences ->
            preferences[KEY_SESSION] = encryptedSession
        }
    }

    override suspend fun getSession(): AuthSession? = withContext(Dispatchers.IO) {
        val encryptedSession = dataStore.data.first()[KEY_SESSION]
            ?: return@withContext null

        runCatching {
            cipher.decrypt(encryptedSession).toAuthSession()
        }.getOrNull()
    }

    override suspend fun clearSession(): Unit = withContext(Dispatchers.IO) {
        dataStore.edit { preferences ->
            preferences.clear()
        }
    }

    private fun AuthSession.toJson(): String {
        return authSessionJson.encodeToString(this)
    }

    private fun String.toAuthSession(): AuthSession {
        return authSessionJson.decodeFromString<AuthSession>(this)
    }

    private companion object {
        val KEY_SESSION = stringPreferencesKey("session")
    }
}
