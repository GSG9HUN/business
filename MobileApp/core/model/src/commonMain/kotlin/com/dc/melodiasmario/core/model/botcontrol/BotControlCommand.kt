package com.dc.melodiasmario.core.model.botcontrol

/**
 * Represents bot control command shared between application layers.
 *
 * @property commandId Command id.
 * @property type Type.
 * @property state State.
 * @property errorMessage Error message.
 */
data class BotControlCommand(
    val commandId: String,
    val type: String,
    val state: String,
    val errorMessage: String? = null
)
