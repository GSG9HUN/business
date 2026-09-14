package com.dc.melodiasmario.core.network.auth.dto

import kotlinx.serialization.Serializable

@Serializable
data class RefreshSessionRequestDto(
    val refreshToken: String,
)
