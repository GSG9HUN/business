package com.dc.melodiasmario.feature.playlisttrack.ui

import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.rememberUpdatedState
import androidx.compose.ui.Modifier
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import com.dc.melodiasmario.core.commonui.feedback.state.MToastHostState
import com.dc.melodiasmario.feature.playlisttrack.presentation.PlaylistTrackEffect
import com.dc.melodiasmario.feature.playlisttrack.presentation.PlaylistTrackEvent
import com.dc.melodiasmario.feature.playlisttrack.presentation.PlaylistTrackViewModel
import com.dc.melodiasmario.feature.playlisttrack.ui.components.playlistTrackEffectMessages
import org.koin.compose.viewmodel.koinViewModel

/**
 * Connects playlist track presentation state and effects to its screen callbacks.
 */
@Composable
fun PlaylistTrackRoute(
    modifier: Modifier = Modifier,
    playlistTrackViewModel: PlaylistTrackViewModel = koinViewModel(),
    guildId: String,
    playlistId: String,
    onBack: () -> Unit,
    toastHostState: MToastHostState
): Unit {
    val uiState = playlistTrackViewModel.uiState.collectAsStateWithLifecycle()
    val effectMessages = playlistTrackEffectMessages()

    val currentMessages by rememberUpdatedState(newValue = effectMessages)
    val currentOnBack by rememberUpdatedState(newValue = onBack)

    LaunchedEffect(key1 = guildId, key2 = playlistId) {
        playlistTrackViewModel.onEvent(
            event = PlaylistTrackEvent.Load(guildId = guildId, playlistId = playlistId)
        )
    }

    LaunchedEffect(key1 = playlistTrackViewModel, key2 = toastHostState) {
        playlistTrackViewModel.effect.collect { effect ->
            when (effect) {
                PlaylistTrackEffect.NavigateBack -> {
                    currentOnBack()
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

    PlaylistTrackScreen(
        modifier = modifier,
        uiState = uiState.value,
        onEvent = playlistTrackViewModel::onEvent
    )
}
