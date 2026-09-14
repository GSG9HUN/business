package com.dc.melodiasmario.core.commonui.topbar

sealed interface TopBarAction {
    data class Search(
        val onClick: () -> Unit,
        val contentDescription: String,
    ) : TopBarAction

    data class Refresh(
        val onClick: () -> Unit,
        val contentDescription: String,
    ) : TopBarAction

    data class More(
        val onClick: () -> Unit,
        val contentDescription: String,
    ) : TopBarAction
}