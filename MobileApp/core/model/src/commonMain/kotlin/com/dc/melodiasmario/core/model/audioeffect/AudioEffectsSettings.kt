package com.dc.melodiasmario.core.model.audioeffect

/** Confirmed or locally edited sound settings; output volume is deliberately independent.
 * @property speed Playback speed multiplier.
 * @property pitch Pitch multiplier.
 * @property equalizer Fifteen zero-based band gains in source-frequency order.
 * @property effects Complete numeric parameter lists for enabled built-in advanced filters.
 */
data class AudioEffectsSettings(
    val speed: Float = 1f,
    val pitch: Float = 1f,
    val equalizer: List<Float> = List(size = 15) { 0f },
    val effects: Map<String, List<Float>> = emptyMap()
)
