package com.dc.melodiasmario.feature.playlists.ui

import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import com.dc.melodiasmario.feature.playlists.presentation.PlaylistSongsViewModel
import com.dc.melodiasmario.core.ui.components.EmptyRouteScreen
import org.koin.compose.viewmodel.koinViewModel

@Composable
fun PlaylistSongsRoute(
    modifier: Modifier = Modifier,
    viewModel: PlaylistSongsViewModel = koinViewModel(),
    playlistId: String,
) {
    EmptyRouteScreen(
        text = viewModel.emptyText(),
        modifier = modifier,
    )
}
