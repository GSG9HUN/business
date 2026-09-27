package com.dc.melodiasmario.core.model.auth

import kotlinx.serialization.SerialName
import kotlinx.serialization.Serializable
import kotlin.time.Clock

@Serializable
data class AuthSession(
    @SerialName("access_token")
    val accessToken: String,
    @SerialName("refresh_token")
    val refreshToken: String,
    @SerialName("expires_in_seconds")
    val expiresInSeconds: Int,
    @SerialName("expires_at_millis")
    val expiresAtMillis: Long
)

fun AuthSession.isExpiredOrCloseToExpiry(): Boolean{
    val refreshBufferMillis = 60_000L
    val nowMillis = Clock.System.now().toEpochMilliseconds()
    return nowMillis >= expiresAtMillis - refreshBufferMillis
}
