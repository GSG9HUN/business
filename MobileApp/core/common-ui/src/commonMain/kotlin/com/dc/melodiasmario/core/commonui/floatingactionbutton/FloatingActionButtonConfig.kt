package com.dc.melodiasmario.core.commonui.floatingactionbutton

/**
 * Provides floating action button config for the floatingactionbutton layer.
 *
 * @property icon Icon.
 * @property contentDescription Accessibility description announced for this control.
 * @property onClick Callback invoked when the enabled control is activated.
 * @property enabled Whether the control accepts interaction.
 */
data class FloatingActionButtonConfig(
    val icon: FloatingActionButtonIcon,
    val contentDescription: String,
    val onClick: () -> Unit,
    val enabled: Boolean = true
)
