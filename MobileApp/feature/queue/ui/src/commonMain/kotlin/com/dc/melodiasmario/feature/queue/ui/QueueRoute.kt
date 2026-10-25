package com.dc.melodiasmario.feature.queue.ui

import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.rememberUpdatedState
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.compose.ui.Modifier
import com.dc.melodiasmario.feature.queue.presentation.QueueEvent
import com.dc.melodiasmario.feature.queue.presentation.QueueViewModel
import org.koin.compose.viewmodel.koinViewModel
import com.dc.melodiasmario.core.commonui.feedback.model.MToastData
import com.dc.melodiasmario.core.commonui.feedback.model.MToastType
import com.dc.melodiasmario.core.commonui.feedback.state.MToastHostState
import com.dc.melodiasmario.feature.queue.presentation.QueueEffect
import com.dc.melodiasmario.feature.queue.generated.resources.Res
import com.dc.melodiasmario.feature.queue.generated.resources.queue_command_succeeded
import com.dc.melodiasmario.feature.queue.generated.resources.queue_command_failed
import com.dc.melodiasmario.feature.queue.generated.resources.queue_playlist_created
import com.dc.melodiasmario.feature.queue.ui.components.queueEffectMessages
import org.jetbrains.compose.resources.stringResource

@Composable
fun QueueRoute(
    modifier: Modifier = Modifier,
    viewModel: QueueViewModel = koinViewModel(),
    guildId: String,
    onGuildClicked: () -> Unit,
    toastHostState: MToastHostState,
) {
    val uiState = viewModel.uiState.collectAsStateWithLifecycle()
    val effectMessages = queueEffectMessages()

    val currentMessages by rememberUpdatedState(effectMessages)
    val currentOnGuildClicked by rememberUpdatedState(onGuildClicked)

    LaunchedEffect(guildId) {
        viewModel.onEvent(
            QueueEvent.LoadQueue(guildId)
        )
    }

    LaunchedEffect(viewModel, toastHostState) {
        viewModel.effect.collect { effect ->
            when (effect) {
                QueueEffect.NavigateToGuildSelector -> {
                    currentOnGuildClicked()
                }

                else -> {
                    currentMessages.toastDataFor(effect)?.let {
                        toastHostState.showToast(it)
                    }
                }
            }
        }
    }

    QueueScreen(
        modifier = modifier,
        uiState = uiState.value,
        onEvent = viewModel::onEvent,
    )
}
