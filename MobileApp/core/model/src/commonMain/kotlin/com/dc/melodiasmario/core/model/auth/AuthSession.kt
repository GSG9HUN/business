package com.dc.melodiasmario.core.model.auth

import kotlin.time.Clock
import kotlinx.serialization.SerialName
import kotlinx.serialization.Serializable

/**
 * Represents auth session shared between application layers.
 *
 * @property accessToken Access token.
 * @property refreshToken Refresh token.
 * @property expiresInSeconds Expires in seconds.
 * @property expiresAtMillis Expires at millis.
 */
@Serializable
data class AuthSession(
    @SerialName(value = "access_token")
    val accessToken: String,
    @SerialName(value = "refresh_token")
    val refreshToken: String,
    @SerialName(value = "expires_in_seconds")
    val expiresInSeconds: Int,
    @SerialName(value = "expires_at_millis")
    val expiresAtMillis: Long
)

/**
 * Checks whether the session requires refresh within the configured expiry margin.
 */
fun AuthSession.isExpiredOrCloseToExpiry(): Boolean{
    val refreshBufferMillis = 60_000L
    val nowMillis = Clock.System.now()
        .toEpochMilliseconds()
    return nowMillis >= expiresAtMillis - refreshBufferMillis
}
