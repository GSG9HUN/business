package com.dc.melodiasmario.core.commonui.data

import org.jetbrains.compose.resources.StringResource

/**
 * Provides bottom bar item for the data layer.
 *
 * @property id Id.
 * @property label Label.
 * @property icon Icon.
 * @property selected Selected.
 * @property onClick Callback invoked when the enabled control is activated.
 */
data class BottomBarItem(
    val id: String,
    val label: StringResource,
    val icon: BottomBarIcon,
    val selected: Boolean,
    val onClick: () -> Unit
)
