package com.dc.melodiasmario.shared.ui.screen.removesong

import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import com.dc.melodiasmario.shared.presentation.removesong.RemoveSongViewModel
import com.dc.melodiasmario.shared.ui.screen.common.EmptyRouteScreen
import org.koin.compose.viewmodel.koinViewModel

@Composable
fun RemoveSongRoute(
    modifier: Modifier = Modifier,
    viewModel: RemoveSongViewModel = koinViewModel(),
) {
    EmptyRouteScreen(
        text = viewModel.emptyText(),
        modifier = modifier,
    )
}
