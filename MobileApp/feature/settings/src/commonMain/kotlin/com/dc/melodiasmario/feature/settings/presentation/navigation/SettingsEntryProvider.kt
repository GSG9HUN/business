package com.dc.melodiasmario.feature.settings.presentation.navigation

import androidx.compose.runtime.Composable
import com.dc.melodiasmario.feature.settings.ui.SettingsRoute

class SettingsEntryProvider {
    @Composable
    fun Entry(
        guildId: String,
    ) {
        SettingsRoute(guildId = guildId)
    }
}
