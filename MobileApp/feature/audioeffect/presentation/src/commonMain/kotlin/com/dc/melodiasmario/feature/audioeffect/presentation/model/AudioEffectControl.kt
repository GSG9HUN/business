package com.dc.melodiasmario.feature.audioeffect.presentation.model

/** Defines the primary advanced audio controls while preserving other confirmed filter parameters.
 * @property filterName Stable built-in filter command identifier.
 * @property defaults Complete parameter list used when enabling a filter.
 * @property primaryIndex Zero-based parameter edited by the primary audio control.
 * @property minimum Minimum enabled primary value accepted by the backend.
 * @property maximum Upper bound of the primary control.
 */
enum class AudioEffectControl(
    val filterName: String,
    val defaults: List<Float>,
    val primaryIndex: Int,
    val minimum: Float,
    val maximum: Float
) {
    Karaoke(filterName = "karaoke", defaults = listOf(.5f, 1f, 220f, 100f), primaryIndex = 0, minimum = 0f, maximum = 1f),
    Rotation(filterName = "rotation", defaults = listOf(.2f), primaryIndex = 0, minimum = -1f, maximum = 1f),
    ChannelMix(filterName = "channelmix", defaults = listOf(1f, 0f, 0f, 1f), primaryIndex = 1, minimum = 0f, maximum = 1f),
    LowPass(filterName = "lowpass", defaults = listOf(10f), primaryIndex = 0, minimum = 1.01f, maximum = 20f),
    Tremolo(filterName = "tremolo", defaults = listOf(2f, .5f), primaryIndex = 1, minimum = .01f, maximum = 1f),
    Vibrato(filterName = "vibrato", defaults = listOf(2f, .5f), primaryIndex = 1, minimum = .01f, maximum = 1f),
    Distortion(
        filterName = "distortion",
        defaults = listOf(0f, 1f, 0f, 1f, 0f, 1f, 0f, 1f),
        primaryIndex = 7,
        minimum = 0f,
        maximum = 2f
    ),
    Rate(filterName = "rate", defaults = listOf(1f), primaryIndex = 0, minimum = .5f, maximum = 2f)
}
