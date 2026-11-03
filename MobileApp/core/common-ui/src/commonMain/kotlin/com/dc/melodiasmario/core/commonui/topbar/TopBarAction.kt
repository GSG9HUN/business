package com.dc.melodiasmario.core.commonui.topbar

/**
 * Provides top bar action for the topbar layer.
 */
sealed interface TopBarAction {
    /**
     * Provides search for the topbar layer.
     *
     * @property onClick Callback invoked when the enabled control is activated.
     * @property contentDescription Accessibility description announced for this control.
     */
    data class Search(
        val onClick: () -> Unit,
        val contentDescription: String
    ) : TopBarAction

    /**
     * Provides refresh for the topbar layer.
     *
     * @property onClick Callback invoked when the enabled control is activated.
     * @property contentDescription Accessibility description announced for this control.
     */
    data class Refresh(
        val onClick: () -> Unit,
        val contentDescription: String
    ) : TopBarAction

    /**
     * Provides more for the topbar layer.
     *
     * @property onClick Callback invoked when the enabled control is activated.
     * @property contentDescription Accessibility description announced for this control.
     */
    data class More(
        val onClick: () -> Unit,
        val contentDescription: String
    ) : TopBarAction

    /**
     * Provides profile for the topbar layer.
     *
     * @property name Name.
     * @property imageUrl Image url.
     * @property onClick Callback invoked when the enabled control is activated.
     * @property contentDescription Accessibility description announced for this control.
     */
    data class Profile(
        val name: String,
        val imageUrl: String?,
        val onClick: () -> Unit,
        val contentDescription: String
    ) : TopBarAction
}
