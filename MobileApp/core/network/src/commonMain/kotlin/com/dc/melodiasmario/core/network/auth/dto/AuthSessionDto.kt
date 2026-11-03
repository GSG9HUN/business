package com.dc.melodiasmario.core.network.auth.dto

import com.dc.melodiasmario.core.model.auth.AuthSession
import kotlinx.serialization.Serializable

/**
 * Represents auth session dto in the API transport contract.
 *
 * @property accessToken Access token.
 * @property refreshToken Refresh token.
 * @property expiresInSeconds Expires in seconds.
 * @property expiresAtMillis Expires at millis.
 */
@Serializable
data class AuthSessionDto(
    val accessToken: String,
    val refreshToken: String,
    val expiresInSeconds: Int,
    val expiresAtMillis: Long
) {
    /**
     * Maps this value to domain.
     */
    fun toDomain(): AuthSession = AuthSession(
        accessToken = accessToken,
        refreshToken = refreshToken,
        expiresInSeconds = expiresInSeconds,
        expiresAtMillis = expiresAtMillis
    )
}
