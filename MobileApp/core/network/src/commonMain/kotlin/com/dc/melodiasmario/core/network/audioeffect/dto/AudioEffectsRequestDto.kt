package com.dc.melodiasmario.core.network.audioeffect.dto

import kotlinx.serialization.Serializable

/** Bounded filter operations executed atomically by the existing guild player service. */
@Serializable
data class AudioEffectsRequestDto(val commands: List<String>)
