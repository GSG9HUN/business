package com.dc.melodiasmario.feature.queue.ui

import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.Composable
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.compose.ui.Modifier
import com.dc.melodiasmario.feature.queue.presentation.QueueEvent
import com.dc.melodiasmario.feature.queue.presentation.QueueViewModel
import org.koin.compose.viewmodel.koinViewModel

@Composable
fun QueueRoute(
    modifier: Modifier = Modifier,
    viewModel: QueueViewModel = koinViewModel(),
    guildId: String,
) {
    val uiState = viewModel.uiState.collectAsStateWithLifecycle()

    LaunchedEffect(guildId) {
        viewModel.onEvent(QueueEvent.LoadQueue(guildId))
    }

    QueueScreen(
        modifier = modifier,
        uiState = uiState.value,
        onEvent = viewModel::onEvent,
    )
}
