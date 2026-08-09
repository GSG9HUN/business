package com.dc.melodiasmario.shared.ui.screen.playlists

import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import org.koin.compose.viewmodel.koinViewModel
import com.dc.melodiasmario.shared.presentation.playlists.PlaylistsViewModel
import com.dc.melodiasmario.shared.ui.screen.common.EmptyRouteScreen

@Composable
fun PlaylistsRoute(
    guildId: String,
    modifier: Modifier = Modifier,
    viewModel: PlaylistsViewModel = koinViewModel(),
) {
    EmptyRouteScreen(
        text = viewModel.emptyText(),
        modifier = modifier,
    )
}
