package com.dc.melodiasmario.feature.settings.presentation.navigation

import androidx.navigation3.runtime.EntryProviderScope
import com.dc.melodiasmario.core.common.navigation.AppRoute
import com.dc.melodiasmario.feature.settings.ui.SettingsRoute

fun EntryProviderScope<AppRoute>.settingsEntry() {
    entry<AppRoute.Settings> { route ->
        SettingsRoute(guildId = route.guildId)
    }
}
