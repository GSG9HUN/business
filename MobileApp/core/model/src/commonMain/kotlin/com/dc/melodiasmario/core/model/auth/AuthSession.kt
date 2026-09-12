package com.dc.melodiasmario.core.model.auth

import kotlin.time.Clock

data class AuthSession(
    val accessToken: String,
    val refreshToken: String,
    val expiresInSeconds: Int,
    val expiresAtMillis: Long
)

fun AuthSession.isExpiredOrCloseToExpiry(): Boolean{
    val refreshBufferMillis = 60_000L
    val nowMillis = Clock.System.now().toEpochMilliseconds()
    return nowMillis >= expiresAtMillis - refreshBufferMillis
}
