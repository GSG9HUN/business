package com.dc.melodiasmario.feature.removesong.presentation.navigation

import androidx.compose.runtime.Composable
import com.dc.melodiasmario.feature.removesong.ui.RemoveSongRoute

class RemoveSongEntryProvider {
    @Composable
    fun Entry(
        guildId: String,
    ) {
        RemoveSongRoute(guildId = guildId)
    }
}
