package com.dc.melodiasmario.feature.playlistsong.ui

import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import com.dc.melodiasmario.core.commonui.components.EmptyRouteScreen
import com.dc.melodiasmario.feature.playlistsong.presentation.PlaylistSongViewModel
import org.koin.compose.viewmodel.koinViewModel

@Composable
fun PlaylistSongRoute(
    modifier: Modifier = Modifier,
    viewModel: PlaylistSongViewModel = koinViewModel(),
    guildId: String,
) {
    EmptyRouteScreen(
        text = viewModel.emptyText(),
        modifier = modifier,
    )
}
