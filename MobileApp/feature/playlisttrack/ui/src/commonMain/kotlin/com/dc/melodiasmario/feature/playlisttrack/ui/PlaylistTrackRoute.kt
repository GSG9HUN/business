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

@Composable
fun PlaylistTrackRoute(
    modifier: Modifier = Modifier,
    viewModel: PlaylistTrackViewModel = koinViewModel(),
    guildId: String,
    playlistId: String,
    onBack: () -> Unit,
    toastHostState: MToastHostState,
) {
    val uiState = viewModel.uiState.collectAsStateWithLifecycle()
    val effectMessages = playlistTrackEffectMessages()

    val currentMessages by rememberUpdatedState(effectMessages)
    val currentOnBack by rememberUpdatedState(onBack)

    LaunchedEffect(guildId, playlistId) {
        viewModel.onEvent(
            PlaylistTrackEvent.Load(guildId, playlistId)
        )
    }

    LaunchedEffect(viewModel, toastHostState) {
        viewModel.effect.collect { effect ->
            when (effect) {
                PlaylistTrackEffect.NavigateBack -> {
                    currentOnBack()
                }

                else -> {
                    currentMessages.toastDataFor(effect)?.let {
                        toastHostState.showToast(it)
                    }
                }
            }
        }
    }

    PlaylistTrackScreen(
        modifier = modifier,
        uiState = uiState.value,
        onEvent = viewModel::onEvent,
    )
}
