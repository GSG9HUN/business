package com.dc.melodiasmario.core.datastore.auth

/**
 * Provides auth session cipher for the auth layer.
 */
actual open class AuthSessionCipher {
    /**
     * Encrypts plaintext into the platform authentication-storage payload.
     */
    actual open fun encrypt(plainText: String): String = plainText

    /**
     * Decrypts a stored authentication payload back to plaintext.
     */
    actual open fun decrypt(encryptedPayload: String): String = encryptedPayload
}
