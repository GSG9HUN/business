package com.dc.melodiasmario.core.network.audioeffect.dto

import kotlinx.serialization.Serializable

/** Saves named draft settings without applying them to live playback. */
@Serializable
data class SaveAudioEffectPresetRequestDto(
    val name: String,
    val commands: List<String>,
    val overwrite: Boolean
)
