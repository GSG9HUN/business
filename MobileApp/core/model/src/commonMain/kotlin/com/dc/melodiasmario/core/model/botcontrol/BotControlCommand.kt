package com.dc.melodiasmario.core.model.botcontrol

data class BotControlCommand(
    val commandId: String,
    val type: String,
    val state: String,
)
