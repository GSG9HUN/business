package com.dc.melodiasmario.core.commonui.topbar

sealed interface TopBarNavigationIcon {
    data class Back(
        val onClick: () -> Unit,
        val contentDescription: String,
    ) : TopBarNavigationIcon

    data class Avatar(
        val name: String,
        val imageUrl: String?,
        val onClick: () -> Unit,
    ) : TopBarNavigationIcon
}