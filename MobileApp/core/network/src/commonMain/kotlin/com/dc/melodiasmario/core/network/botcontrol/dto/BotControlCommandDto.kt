package com.dc.melodiasmario.core.network.botcontrol.dto

import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import kotlinx.serialization.Serializable

@Serializable
data class BotControlCommandDto(
    val commandId: String,
    val type: String,
    val state: String,
) {
    fun toDomain() = BotControlCommand(
        commandId = commandId,
        type = type,
        state = state,
    )
}
