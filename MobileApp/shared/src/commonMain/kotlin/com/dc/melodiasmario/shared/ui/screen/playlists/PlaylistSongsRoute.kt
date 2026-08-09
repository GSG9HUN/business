package com.dc.melodiasmario.shared.ui.screen.playlists

import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import com.dc.melodiasmario.shared.presentation.playlists.PlaylistSongsViewModel
import com.dc.melodiasmario.shared.ui.screen.common.EmptyRouteScreen
import org.koin.compose.viewmodel.koinViewModel

@Composable
fun PlaylistSongsRoute(
    modifier: Modifier = Modifier,
    viewModel: PlaylistSongsViewModel = koinViewModel(),
) {
    EmptyRouteScreen(
        text = viewModel.emptyText(),
        modifier = modifier,
    )
}
