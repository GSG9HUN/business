package com.dc.melodiasmario.feature.queue.ui

import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import com.dc.melodiasmario.feature.queue.presentation.QueueViewModel
import com.dc.melodiasmario.core.ui.components.EmptyRouteScreen
import org.koin.compose.viewmodel.koinViewModel

@Composable
fun QueueRoute(
    modifier: Modifier = Modifier,
    viewModel: QueueViewModel = koinViewModel(),
    guildId: String,
) {
    EmptyRouteScreen(
        text = viewModel.emptyText(),
        modifier = modifier,
    )
}
