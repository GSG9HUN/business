package com.dc.melodiasmario.core.commonui.feedback.model

/**
 * Represents toast data shared between application layers.
 *
 * @property message Message.
 * @property type Type.
 * @property durationMillis Duration millis.
 */
data class MToastData(
    val message: String,
    val type: MToastType,
    val durationMillis: Long = 3000
)
