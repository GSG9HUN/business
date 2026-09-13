package com.dc.melodiasmario.feature.playlistsong.presentation.navigation

import androidx.navigation3.runtime.EntryProviderScope
import com.dc.melodiasmario.core.common.navigation.AppRoute
import com.dc.melodiasmario.feature.playlistsong.ui.PlaylistSongRoute

fun EntryProviderScope<AppRoute>.playlistSongEntry() {
    entry<AppRoute.PlaylistSong> { route ->
        PlaylistSongRoute(guildId = route.guildId)
    }
}
