package com.dc.melodiasmario.feature.removesong.presentation.navigation

import androidx.navigation3.runtime.EntryProviderScope
import com.dc.melodiasmario.core.common.navigation.AppRoute
import com.dc.melodiasmario.feature.removesong.ui.RemoveSongRoute

fun EntryProviderScope<AppRoute>.removeSongEntry() {
    entry<AppRoute.RemoveSong> { route ->
        RemoveSongRoute(guildId = route.guildId)
    }
}
