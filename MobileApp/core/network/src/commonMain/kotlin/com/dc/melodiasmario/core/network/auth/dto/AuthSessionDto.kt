package com.dc.melodiasmario.core.network.auth.dto

import com.dc.melodiasmario.core.model.auth.AuthSession
import kotlinx.serialization.Serializable

@Serializable
data class AuthSessionDto(
    val accessToken: String,
    val refreshToken: String,
    val expiresInSeconds: Int,
    val expiresAtMillis: Long,
)

fun AuthSessionDto.toDomain() = AuthSession(
    accessToken = accessToken,
    refreshToken = refreshToken,
    expiresInSeconds = expiresInSeconds,
    expiresAtMillis = expiresAtMillis,
)
