package com.dc.melodiasmario.core.commonui.topbar

data class TopBarConfig(
    val title: String,
    val subTitle: String? = null,
    val navigationIcon: TopBarNavigationIcon? = null,
    val actions: List<TopBarAction> = emptyList(),
)



