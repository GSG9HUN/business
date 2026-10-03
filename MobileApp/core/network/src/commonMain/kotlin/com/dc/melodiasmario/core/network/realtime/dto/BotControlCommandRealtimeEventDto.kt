package com.dc.melodiasmario.core.network.realtime.dto

import com.dc.melodiasmario.core.model.realtime.BotControlCommandRealtimeEvent
import kotlinx.serialization.Serializable
import kotlinx.serialization.json.Json
import kotlinx.serialization.json.jsonObject
import kotlinx.serialization.json.jsonPrimitive

@Serializable
data class BotControlCommandRealtimeEventDto(
    val commandId: String,
    val guildId: String,
    val userId: String,
    val type: String,
    val state: String,
    val errorMessage: String?,
    val resultJson: String?,
    val createdAtUtc: String,
    val claimedAtUtc: String?,
    val completedAtUtc: String?,
    val eventName: String = "",
) {
    fun toDomain(eventNameOverride: String? = null) = BotControlCommandRealtimeEvent(
        commandId = commandId,
        guildId = guildId,
        userId = userId,
        type = type,
        state = state,
        errorKey = resultJson.toErrorKey(),
        errorMessage = errorMessage,
        resultJson = resultJson,
        createdAtUtc = createdAtUtc,
        claimedAtUtc = claimedAtUtc,
        completedAtUtc = completedAtUtc,
        eventName = eventNameOverride ?: eventName
    )

    private fun String?.toErrorKey(): String? {
        if (isNullOrBlank()) return null

        return runCatching {
            Json.parseToJsonElement(this)
                .jsonObject["errorCode"]
                ?.jsonPrimitive
                ?.content
                ?.takeIf { it.isNotBlank() }
        }.getOrNull()
    }
}
