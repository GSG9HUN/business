package com.dc.melodiasmario.core.datastore.auth

import android.security.keystore.KeyGenParameterSpec
import android.security.keystore.KeyProperties
import android.util.Base64
import org.json.JSONObject
import java.security.KeyStore
import javax.crypto.Cipher
import javax.crypto.KeyGenerator
import javax.crypto.SecretKey
import javax.crypto.spec.GCMParameterSpec

actual open class AuthSessionCipher {
    private var cachedSecretKey: SecretKey? = null

    actual open fun encrypt(plainText: String): String {
        val cipher = Cipher.getInstance(TRANSFORMATION)
        cipher.init(Cipher.ENCRYPT_MODE, getOrCreateSecretKey())

        val cipherText = cipher.doFinal(plainText.toByteArray(Charsets.UTF_8))
        val iv = cipher.iv

        return JSONObject()
            .put(JSON_IV, Base64.encodeToString(iv, Base64.NO_WRAP))
            .put(JSON_CIPHER_TEXT, Base64.encodeToString(cipherText, Base64.NO_WRAP))
            .toString()
    }

    actual open fun decrypt(encryptedPayload: String): String {
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
        cachedSecretKey?.let { return it }

        val keyStore = KeyStore.getInstance(ANDROID_KEYSTORE).apply {
            load(null)
        }

        val existingKey = keyStore.getKey(KEY_ALIAS, null) as? SecretKey
        if (existingKey != null) {
            cachedSecretKey = existingKey
            return existingKey
        }

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
        return keyGenerator.generateKey().also { cachedSecretKey = it }
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
