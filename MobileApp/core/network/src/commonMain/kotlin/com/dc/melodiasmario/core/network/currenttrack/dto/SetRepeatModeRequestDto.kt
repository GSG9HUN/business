package com.dc.melodiasmario.core.network.currenttrack.dto

import kotlinx.serialization.Serializable

/**
 * Represents set repeat mode request dto in the API transport contract.
 *
 * @property mode Mode.
 */
@Serializable
data class SetRepeatModeRequestDto(
    val mode: String
)
