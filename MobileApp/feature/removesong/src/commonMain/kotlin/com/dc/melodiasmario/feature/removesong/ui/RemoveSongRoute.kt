package com.dc.melodiasmario.feature.removesong.ui

import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import com.dc.melodiasmario.feature.removesong.presentation.RemoveSongViewModel
import com.dc.melodiasmario.core.ui.components.EmptyRouteScreen
import org.koin.compose.viewmodel.koinViewModel

@Composable
fun RemoveSongRoute(
    modifier: Modifier = Modifier,
    viewModel: RemoveSongViewModel = koinViewModel(),
    guildId: String,
) {
    EmptyRouteScreen(
        text = viewModel.emptyText(),
        modifier = modifier,
    )
}
