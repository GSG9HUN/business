package com.dc.melodiasmario.feature.playlists.ui

import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import org.koin.compose.viewmodel.koinViewModel
import com.dc.melodiasmario.feature.playlists.presentation.PlaylistsViewModel
import com.dc.melodiasmario.core.ui.components.EmptyRouteScreen

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
