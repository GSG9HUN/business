package com.dc.melodiasmario.core.datastore.auth

actual open class AuthSessionCipher {
    actual open fun encrypt(plainText: String): String = plainText

    actual open fun decrypt(encryptedPayload: String): String = encryptedPayload
}
