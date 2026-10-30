package com.dc.melodiasmario.feature.settings.presentation.navigation

import androidx.navigation3.runtime.EntryProviderScope
import com.dc.melodiasmario.core.common.navigation.AppRoute
import com.dc.melodiasmario.feature.settings.ui.SettingsRoute

/**
 * Registers the settings destination and its navigation callbacks.
 */
fun EntryProviderScope<AppRoute>.settingsEntry(): Unit {
    entry<AppRoute.Settings> { route ->
        SettingsRoute(guildId = route.guildId)
    }
}
