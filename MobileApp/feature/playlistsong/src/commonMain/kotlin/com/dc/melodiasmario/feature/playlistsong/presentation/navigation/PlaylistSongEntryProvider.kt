package com.dc.melodiasmario.feature.playlistsong.presentation.navigation

import androidx.compose.runtime.Composable
import com.dc.melodiasmario.feature.playlistsong.ui.PlaylistSongRoute

class PlaylistSongEntryProvider {
    @Composable
    fun Entry(
        guildId: String,
    ) {
        PlaylistSongRoute(guildId = guildId)
    }
}
