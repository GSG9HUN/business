package com.dc.melodiasmario.feature.currentmusic.presentation.navigation

import androidx.navigation3.runtime.EntryProviderScope
import com.dc.melodiasmario.core.common.navigation.AppRoute
import com.dc.melodiasmario.feature.currentmusic.ui.CurrentMusicRoute

fun EntryProviderScope<AppRoute>.currentMusicEntry() {
    entry<AppRoute.CurrentMusic> { route ->
        CurrentMusicRoute(guildId = route.guildId)
    }
}
