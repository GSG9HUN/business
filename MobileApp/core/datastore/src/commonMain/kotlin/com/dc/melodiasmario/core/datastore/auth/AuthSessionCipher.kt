package com.dc.melodiasmario.core.datastore.auth

expect open class AuthSessionCipher() {
    open fun encrypt(plainText: String): String
    open fun decrypt(encryptedPayload: String): String
}
