package com.dc.melodiasmario.feature.addsong.ui

import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import com.dc.melodiasmario.feature.addsong.presentation.AddSongViewModel
import com.dc.melodiasmario.core.ui.components.EmptyRouteScreen
import org.koin.compose.viewmodel.koinViewModel

@Composable
fun AddSongRoute(
    modifier: Modifier = Modifier,
    viewModel: AddSongViewModel = koinViewModel(),
    guildId: String,
) {
    EmptyRouteScreen(
        text = viewModel.emptyText(),
        modifier = modifier,
    )
}
