package com.dc.melodiasmario.feature.playlist.ui.playlistsong

import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import com.dc.melodiasmario.core.commonui.components.EmptyRouteScreen

@Composable
fun PlaylistSongsRoute(
    guildId: String,
    playlistId: String,
    modifier: Modifier = Modifier,
) {
    EmptyRouteScreen(
        text = playlistId,
        modifier = modifier,
    )
}
