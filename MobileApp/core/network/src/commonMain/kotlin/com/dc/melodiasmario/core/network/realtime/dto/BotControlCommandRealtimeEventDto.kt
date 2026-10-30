package com.dc.melodiasmario.core.network.realtime.dto

import com.dc.melodiasmario.core.model.realtime.BotControlCommandRealtimeEvent
import kotlinx.serialization.Serializable
import kotlinx.serialization.json.Json
import kotlinx.serialization.json.jsonObject
import kotlinx.serialization.json.jsonPrimitive

/**
 * Represents bot control command realtime event dto in the API transport contract.
 *
 * @property commandId Command id.
 * @property guildId Identifier of the guild owning this value.
 * @property userId User id.
 * @property type Type.
 * @property state State.
 * @property errorMessage Error message.
 * @property resultJson Result json.
 * @property createdAtUtc Created at utc.
 * @property claimedAtUtc Claimed at utc.
 * @property completedAtUtc Completed at utc.
 * @property eventName Event name.
 */
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
    val eventName: String = ""
) {
    /**
     * Maps this value to domain.
     */
    fun toDomain(eventNameOverride: String? = null): BotControlCommandRealtimeEvent = BotControlCommandRealtimeEvent(
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
        if (isNullOrBlank()) {
            return null
        }

        return runCatching {
            Json.parseToJsonElement(this)
                .jsonObject["errorCode"]
                ?.jsonPrimitive
                ?.content
                ?.takeIf { textValue -> textValue.isNotBlank() }
        }.getOrNull()
    }
}
