package com.dc.melodiasmario.core.network.audioeffect.dto

import com.dc.melodiasmario.core.model.audioeffect.AudioEffectsSettings
import com.dc.melodiasmario.core.model.audioeffect.AudioEffectsSnapshot
import kotlinx.serialization.Serializable
import kotlinx.serialization.json.JsonArray
import kotlinx.serialization.json.JsonObject
import kotlinx.serialization.json.JsonPrimitive
import kotlinx.serialization.json.floatOrNull
import kotlinx.serialization.json.intOrNull

/** Transports the authoritative Lavalink built-in filter map and guild-local preset metadata. */
@Serializable
data class AudioEffectsDto(
    val filters: JsonObject? = null,
    val presets: List<String> = emptyList(),
    val canManagePresets: Boolean = false,
    val page: Int = 1,
    val pages: Int = 1
) {

    /** Maps optional built-in filters without leaking JSON or plugin payloads into the domain. */
    fun toDomain(): AudioEffectsSnapshot {
        val settings = filters?.let { filterMap ->
            val timescale = filterMap["timescale"] as? JsonObject
            val equalizer = MutableList(size = 15) { 0f }
            (filterMap["equalizer"] as? JsonArray)?.forEach { bandElement ->
                val band = bandElement as? JsonObject
                val index = (band?.get(key = "band") as? JsonPrimitive)?.intOrNull

                if (index != null && index in equalizer.indices) {
                    equalizer[index] = band.number(name = "gain", defaultValue = 0f)
                }
            }

            val effectFields = mapOf(
                "karaoke" to listOf("level", "monoLevel", "filterBand", "filterWidth"),
                "rotation" to listOf("rotationHz"),
                "channelmix" to listOf("leftToLeft", "leftToRight", "rightToLeft", "rightToRight"),
                "lowpass" to listOf("smoothing"),
                "tremolo" to listOf("frequency", "depth"),
                "vibrato" to listOf("frequency", "depth"),
                "distortion" to listOf(
                    "sinOffset",
                    "sinScale",
                    "cosOffset",
                    "cosScale",
                    "tanOffset",
                    "tanScale",
                    "offset",
                    "scale"
                )
            )
            val effects = buildMap {
                effectFields.forEach { (effectName, fields) ->
                    val jsonKey = when (effectName) {
                        "channelmix" -> "channelMix"
                        "lowpass" -> "lowPass"
                        else -> effectName
                    }
                    val effectMap = filterMap[jsonKey] as? JsonObject

                    if (effectMap != null) {
                        put(
                            key = effectName,
                            value = fields.map { fieldName -> effectMap.number(name = fieldName, defaultValue = 0f) })
                    }
                }
                val rate = timescale.number(name = "rate", defaultValue = 1f)

                if (rate != 1f) {
                    put(key = "rate", value = listOf(rate))
                }
            }
            AudioEffectsSettings(
                speed = timescale.number(name = "speed", defaultValue = 1f),
                pitch = timescale.number(name = "pitch", defaultValue = 1f),
                equalizer = equalizer,
                effects = effects
            )
        }
        return AudioEffectsSnapshot(
            settings = settings,
            presets = presets,
            canManagePresets = canManagePresets,
            page = page,
            pages = pages
        )
    }
}

private fun JsonObject?.number(name: String, defaultValue: Float): Float =
    (this?.get(key = name) as? JsonPrimitive)?.floatOrNull ?: defaultValue
