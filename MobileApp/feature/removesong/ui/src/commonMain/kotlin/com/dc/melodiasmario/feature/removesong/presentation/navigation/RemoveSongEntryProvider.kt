package com.dc.melodiasmario.feature.removesong.presentation.navigation

import androidx.navigation3.runtime.EntryProviderScope
import com.dc.melodiasmario.core.common.navigation.AppRoute
import com.dc.melodiasmario.feature.removesong.ui.RemoveSongRoute

/**
 * Registers the remove song destination and its navigation callbacks.
 */
fun EntryProviderScope<AppRoute>.removeSongEntry(): Unit {
    entry<AppRoute.RemoveSong> { route ->
        RemoveSongRoute(guildId = route.guildId)
    }
}
