package com.dc.melodiasmario.shared.ui.screen.currentmusic

import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import com.dc.melodiasmario.shared.presentation.currentmusic.CurrentMusicViewModel
import com.dc.melodiasmario.shared.ui.screen.common.EmptyRouteScreen
import org.koin.compose.viewmodel.koinViewModel

@Composable
fun CurrentMusicRoute(
    modifier: Modifier = Modifier,
    viewModel: CurrentMusicViewModel = koinViewModel(),
) {
    EmptyRouteScreen(
        text = viewModel.emptyText(),
        modifier = modifier,
    )
}
