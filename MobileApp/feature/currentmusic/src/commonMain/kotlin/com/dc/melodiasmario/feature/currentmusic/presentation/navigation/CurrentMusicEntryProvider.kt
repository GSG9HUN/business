package com.dc.melodiasmario.feature.currentmusic.presentation.navigation

import androidx.compose.runtime.Composable
import com.dc.melodiasmario.feature.currentmusic.ui.CurrentMusicRoute

class CurrentMusicEntryProvider {
    @Composable
    fun Entry(
        guildId: String,
    ) {
        CurrentMusicRoute(guildId = guildId)
    }
}
