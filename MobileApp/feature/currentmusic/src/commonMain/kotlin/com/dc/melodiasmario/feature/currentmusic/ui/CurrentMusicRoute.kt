package com.dc.melodiasmario.feature.currentmusic.ui

import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import com.dc.melodiasmario.feature.currentmusic.presentation.CurrentMusicViewModel
import com.dc.melodiasmario.core.ui.components.EmptyRouteScreen
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
