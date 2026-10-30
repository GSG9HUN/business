package com.dc.melodiasmario.core.commonui.topbar

/**
 * Provides top bar navigation icon for the topbar layer.
 */
sealed interface TopBarNavigationIcon {
    /**
     * Provides back for the topbar layer.
     *
     * @property onClick Callback invoked when the enabled control is activated.
     * @property contentDescription Accessibility description announced for this control.
     */
    data class Back(
        val onClick: () -> Unit,
        val contentDescription: String
    ) : TopBarNavigationIcon

    /**
     * Provides avatar for the topbar layer.
     *
     * @property name Name.
     * @property imageUrl Image url.
     * @property onClick Callback invoked when the enabled control is activated.
     */
    data class Avatar(
        val name: String,
        val imageUrl: String?,
        val onClick: () -> Unit
    ) : TopBarNavigationIcon
}
