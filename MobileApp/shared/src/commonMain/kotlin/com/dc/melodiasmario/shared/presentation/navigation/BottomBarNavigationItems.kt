package com.dc.melodiasmario.shared.presentation.navigation

import com.dc.melodiasmario.core.commonui.data.BottomBarIcon
import com.dc.melodiasmario.core.commonui.data.BottomBarItem
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.Res
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.bottom_bar_now_playing
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.bottom_bar_playlists
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.bottom_bar_queue
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.bottom_bar_settings

fun bottomBarItems(
    currentRoute: AppRoute,
    onNavigate: (AppRoute) -> Unit,
): List<BottomBarItem> {
    val guildId = currentRoute.guildIdOrNull() ?: return emptyList()

    return listOf(
        BottomBarItem(
            id = "current_music",
            label = Res.string.bottom_bar_now_playing,
            icon = BottomBarIcon.NowPlaying,
            selected = currentRoute is AppRoute.CurrentMusic,
            onClick = { onNavigate(AppRoute.CurrentMusic(guildId = guildId)) },
        ),
        BottomBarItem(
            id = "queue",
            label = Res.string.bottom_bar_queue,
            icon = BottomBarIcon.Queue,
            selected = currentRoute is AppRoute.Queue,
            onClick = { onNavigate(AppRoute.Queue(guildId = guildId)) },
        ),
        BottomBarItem(
            id = "playlists",
            label = Res.string.bottom_bar_playlists,
            icon = BottomBarIcon.Playlists,
            selected = currentRoute is AppRoute.Playlists,
            onClick = { onNavigate(AppRoute.Playlists(guildId = guildId)) },
        ),
        BottomBarItem(
            id = "settings",
            label = Res.string.bottom_bar_settings,
            icon = BottomBarIcon.Settings,
            selected = currentRoute is AppRoute.Settings,
            onClick = { onNavigate(AppRoute.Settings(guildId = guildId)) },
        ),
    )
}
