package com.dc.melodiasmario.core.datastore.auth

import android.content.Context
import android.security.keystore.KeyGenParameterSpec
import android.security.keystore.KeyProperties
import android.util.Base64
import androidx.datastore.core.DataStore
import androidx.datastore.preferences.core.Preferences
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.core.stringPreferencesKey
import androidx.datastore.preferences.preferencesDataStore
import com.dc.melodiasmario.core.domain.auth.SecureAuthSessionStorage
import com.dc.melodiasmario.core.model.auth.AuthSession
import kotlinx.coroutines.flow.first
import org.json.JSONObject
import org.koin.core.annotation.Single
import java.security.KeyStore
import javax.crypto.Cipher
import javax.crypto.KeyGenerator
import javax.crypto.SecretKey
import javax.crypto.spec.GCMParameterSpec

private val Context.authSessionDataStore by preferencesDataStore(
    name = "secure_auth_session",
)

@Single(binds = [SecureAuthSessionStorage::class])
class SecureAuthSessionStorageImpl(
    private val context: Context,
) : SecureAuthSessionStorage {
    private var cipher: AuthSessionCipher = AndroidKeystoreAuthSessionCipher()
    private var dataStore: DataStore<Preferences> = context.authSessionDataStore

    internal constructor(
        context: Context,
        cipher: AuthSessionCipher,
        dataStore: DataStore<Preferences>,
    ) : this(context) {
        this.cipher = cipher
        this.dataStore = dataStore
    }

    override suspend fun saveSession(session: AuthSession) {
        val encryptedSession = cipher.encrypt(session.toJson())

        dataStore.edit { preferences ->
            preferences[KEY_SESSION] = encryptedSession
        }
    }

    override suspend fun getSession(): AuthSession? {
        val encryptedSession = dataStore.data.first()[KEY_SESSION]
            ?: return null

        return runCatching {
            cipher.decrypt(encryptedSession).toAuthSession()
        }.getOrNull()
    }

    override suspend fun clearSession() {
        dataStore.edit { preferences ->
            preferences.clear()
        }
    }

    private fun AuthSession.toJson(): String {
        return JSONObject()
            .put(JSON_ACCESS_TOKEN, accessToken)
            .put(JSON_REFRESH_TOKEN, refreshToken)
            .put(JSON_EXPIRES_IN_SECONDS, expiresInSeconds)
            .put(JSON_EXPIRES_AT_MILLIS, expiresAtMillis)
            .toString()
    }

    private fun String.toAuthSession(): AuthSession {
        val json = JSONObject(this)

        return AuthSession(
            accessToken = json.getString(JSON_ACCESS_TOKEN),
            refreshToken = json.getString(JSON_REFRESH_TOKEN),
            expiresInSeconds = json.getInt(JSON_EXPIRES_IN_SECONDS),
            expiresAtMillis = json.getLong(JSON_EXPIRES_AT_MILLIS),
        )
    }

    private companion object {
        val KEY_SESSION = stringPreferencesKey("session")

        const val JSON_ACCESS_TOKEN = "access_token"
        const val JSON_REFRESH_TOKEN = "refresh_token"
        const val JSON_EXPIRES_IN_SECONDS = "expires_in_seconds"
        const val JSON_EXPIRES_AT_MILLIS = "expires_at_millis"
    }
}

interface AuthSessionCipher {
    fun encrypt(plainText: String): String
    fun decrypt(encryptedPayload: String): String
}

class AndroidKeystoreAuthSessionCipher : AuthSessionCipher {
    override fun encrypt(plainText: String): String {
        val cipher = Cipher.getInstance(TRANSFORMATION)
        cipher.init(Cipher.ENCRYPT_MODE, getOrCreateSecretKey())

        val cipherText = cipher.doFinal(plainText.toByteArray(Charsets.UTF_8))
        val iv = cipher.iv

        return JSONObject()
            .put(JSON_IV, Base64.encodeToString(iv, Base64.NO_WRAP))
            .put(JSON_CIPHER_TEXT, Base64.encodeToString(cipherText, Base64.NO_WRAP))
            .toString()
    }

    override fun decrypt(encryptedPayload: String): String {
        val json = JSONObject(encryptedPayload)
        val iv = Base64.decode(json.getString(JSON_IV), Base64.NO_WRAP)
        val cipherText = Base64.decode(json.getString(JSON_CIPHER_TEXT), Base64.NO_WRAP)

        val cipher = Cipher.getInstance(TRANSFORMATION)
        cipher.init(
            Cipher.DECRYPT_MODE,
            getOrCreateSecretKey(),
            GCMParameterSpec(GCM_TAG_LENGTH_BITS, iv),
        )

        return cipher.doFinal(cipherText).toString(Charsets.UTF_8)
    }

    private fun getOrCreateSecretKey(): SecretKey {
        val keyStore = KeyStore.getInstance(ANDROID_KEYSTORE).apply {
            load(null)
        }

        val existingKey = keyStore.getKey(KEY_ALIAS, null) as? SecretKey
        if (existingKey != null) return existingKey

        val keyGenerator = KeyGenerator.getInstance(
            KeyProperties.KEY_ALGORITHM_AES,
            ANDROID_KEYSTORE,
        )

        val keySpec = KeyGenParameterSpec.Builder(
            KEY_ALIAS,
            KeyProperties.PURPOSE_ENCRYPT or KeyProperties.PURPOSE_DECRYPT,
        )
            .setKeySize(KEY_SIZE_BITS)
            .setBlockModes(KeyProperties.BLOCK_MODE_GCM)
            .setEncryptionPaddings(KeyProperties.ENCRYPTION_PADDING_NONE)
            .setRandomizedEncryptionRequired(true)
            .build()

        keyGenerator.init(keySpec)
        return keyGenerator.generateKey()
    }

    private companion object {
        const val ANDROID_KEYSTORE = "AndroidKeyStore"
        const val KEY_ALIAS = "melodias_mario_auth_session_key"
        const val TRANSFORMATION = "AES/GCM/NoPadding"
        const val KEY_SIZE_BITS = 256
        const val GCM_TAG_LENGTH_BITS = 128

        const val JSON_IV = "iv"
        const val JSON_CIPHER_TEXT = "cipher_text"
    }
}
