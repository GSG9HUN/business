package com.dc.melodiasmario.core.commonui.designsystem.components.dialog

/**
 * Provides more action for the dialog layer.
 *
 * @property text Text.
 * @property onClick Callback invoked when the enabled control is activated.
 * @property isDestructive Whether the action uses destructive-action styling.
 * @property enabled Whether the control accepts interaction.
 */
data class MMoreAction(
    val text: String,
    val onClick: () -> Unit,
    val isDestructive: Boolean = false,
    val enabled: Boolean = true
)
