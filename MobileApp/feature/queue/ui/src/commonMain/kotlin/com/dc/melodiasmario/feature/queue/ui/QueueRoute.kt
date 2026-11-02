package com.dc.melodiasmario.feature.queue.ui

import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.rememberUpdatedState
import androidx.compose.ui.Modifier
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import com.dc.melodiasmario.core.commonui.feedback.state.MToastHostState
import com.dc.melodiasmario.feature.queue.presentation.QueueEffect
import com.dc.melodiasmario.feature.queue.presentation.QueueEvent
import com.dc.melodiasmario.feature.queue.presentation.QueueViewModel
import com.dc.melodiasmario.feature.queue.ui.components.queueEffectMessages
import org.koin.compose.viewmodel.koinViewModel

/**
 * Connects queue presentation state and effects to its screen callbacks.
 */
@Composable
fun QueueRoute(
    modifier: Modifier = Modifier,
    queueViewModel: QueueViewModel = koinViewModel(),
    guildId: String,
    onGuildClicked: () -> Unit,
    toastHostState: MToastHostState
): Unit {
    val uiState = queueViewModel.uiState.collectAsStateWithLifecycle()
    val effectMessages = queueEffectMessages()

    val currentMessages by rememberUpdatedState(newValue = effectMessages)
    val currentOnGuildClicked by rememberUpdatedState(newValue = onGuildClicked)

    LaunchedEffect(key1 = guildId) {
        queueViewModel.onEvent(
            event = QueueEvent.LoadQueue(guildId = guildId)
        )
    }

    LaunchedEffect(key1 = queueViewModel, key2 = toastHostState) {
        queueViewModel.effect.collect { effect ->
            when (effect) {
                QueueEffect.NavigateToGuildSelector -> {
                    currentOnGuildClicked()
                }

                else -> {
                    currentMessages.toastDataFor(effect = effect)
                        ?.let { toastData ->
                            toastHostState.showToast(toast = toastData)
                        }
                }
            }
        }
    }

    QueueScreen(
        modifier = modifier,
        uiState = uiState.value,
        onEvent = queueViewModel::onEvent
    )
}
