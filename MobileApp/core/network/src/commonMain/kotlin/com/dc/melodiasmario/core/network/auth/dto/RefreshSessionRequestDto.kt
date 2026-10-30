package com.dc.melodiasmario.core.network.auth.dto

import kotlinx.serialization.Serializable

/**
 * Represents refresh session request dto in the API transport contract.
 *
 * @property refreshToken Refresh token.
 */
@Serializable
data class RefreshSessionRequestDto(
    val refreshToken: String
)
