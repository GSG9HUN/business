package com.dc.melodiasmario.core.network.botcontrol.dto

import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import kotlinx.serialization.Serializable

/**
 * Represents bot control command dto in the API transport contract.
 *
 * @property commandId Command id.
 * @property type Type.
 * @property state State.
 * @property errorMessage Error message.
 */
@Serializable
data class BotControlCommandDto(
    val commandId: String,
    val type: String,
    val state: String,
    val errorMessage: String? = null
) {
    /**
     * Maps this value to domain.
     */
    fun toDomain(): BotControlCommand = BotControlCommand(
        commandId = commandId,
        type = type,
        state = state,
        errorMessage = errorMessage
    )
}
