package com.dc.melodiasmario.core.commonui.floatingactionbutton

data class FloatingActionButtonConfig(
    val icon: FloatingActionButtonIcon,
    val contentDescription: String,
    val onClick: () -> Unit,
    val enabled: Boolean = true,
)

enum class FloatingActionButtonIcon {
    Add,
}
