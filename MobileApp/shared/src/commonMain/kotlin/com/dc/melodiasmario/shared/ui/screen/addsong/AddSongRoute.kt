package com.dc.melodiasmario.shared.ui.screen.addsong

import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import com.dc.melodiasmario.shared.presentation.addsong.AddSongViewModel
import com.dc.melodiasmario.shared.ui.screen.common.EmptyRouteScreen
import org.koin.compose.viewmodel.koinViewModel

@Composable
fun AddSongRoute(
    modifier: Modifier = Modifier,
    viewModel: AddSongViewModel = koinViewModel(),
) {
    EmptyRouteScreen(
        text = viewModel.emptyText(),
        modifier = modifier,
    )
}
