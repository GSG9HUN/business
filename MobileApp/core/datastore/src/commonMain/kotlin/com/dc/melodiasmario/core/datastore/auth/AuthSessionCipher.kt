package com.dc.melodiasmario.core.datastore.auth

/**
 * Provides auth session cipher for the auth layer.
 */
expect open class AuthSessionCipher() {
    /**
     * Encrypts plaintext into the platform authentication-storage payload.
     */
    open fun encrypt(plainText: String): String

    /**
     * Decrypts a stored authentication payload back to plaintext.
     */
    open fun decrypt(encryptedPayload: String): String
}
