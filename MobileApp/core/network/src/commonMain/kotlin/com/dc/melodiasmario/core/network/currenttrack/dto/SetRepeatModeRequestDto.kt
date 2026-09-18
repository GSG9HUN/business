package com.dc.melodiasmario.core.network.currenttrack.dto

import kotlinx.serialization.Serializable

@Serializable
data class SetRepeatModeRequestDto(
    val mode: String,
)
