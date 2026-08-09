package com.dc.melodiasmario.shared.data.auth.remote.dto

import com.dc.melodiasmario.shared.domain.auth.model.AuthSession
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