package com.dc.melodiasmario.core.ui.data

import org.jetbrains.compose.resources.StringResource

data class BottomBarItem(
    val id: String,
    val label: StringResource,
    val icon: BottomBarIcon,
    val selected: Boolean,
    val onClick: () -> Unit,
)

enum class BottomBarIcon {
    NowPlaying,
    Queue,
    Playlists,
    Settings,
}
