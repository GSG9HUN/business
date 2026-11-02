package com.dc.melodiasmario.core.commonui.topbar

/**
 * Provides top bar config for the topbar layer.
 *
 * @property title Title.
 * @property subTitle Sub title.
 * @property navigationIcon Navigation icon.
 * @property actions Actions.
 */
data class TopBarConfig(
    val title: String,
    val subTitle: String? = null,
    val navigationIcon: TopBarNavigationIcon? = null,
    val actions: List<TopBarAction> = emptyList()
)
