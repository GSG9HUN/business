package com.dc.melodiasmario.feature.audioeffect.presentation.mapper

import com.dc.melodiasmario.core.model.audioeffect.AudioEffectsSettings
import kotlin.math.abs

/** Recognizes confirmed built-in profiles; unmatched or composite settings remain custom. */
fun AudioEffectsSettings.profileName(): String? {
    if (effects.isNotEmpty()) {
        return null
    }

    val neutralEq = equalizer.all { gain -> abs(x = gain) < .0001f }

    if (neutralEq
        && abs(x = speed - 1.15f) < .0001f
        && abs(x = pitch - 1.15f) < .0001f) {
        return "nightcore"
    }

    if (abs(x = speed - 1f) > .0001f || abs(x = pitch - 1f) > .0001f) {
        return null
    }

    if (neutralEq) {
        return "flat"
    }

    val factors = listOf(.6f, 1f, 1f, .8f, .4f, .15f)
    return listOf("bass-light" to .10f, "bass-heavy" to .25f).firstOrNull { (_, emphasis) ->
        equalizer.withIndex().all { (band, gain) -> abs(x = gain - (factors.getOrNull(index = band) ?: 0f) * emphasis) < .0001f }
    }?.first
}
