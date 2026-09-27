package com.dc.melodiasmario.feature.currenttrack.ui

import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import com.dc.melodiasmario.core.commonui.feedback.state.MToastHostState
import com.dc.melodiasmario.feature.currenttrack.presentation.CurrentTrackEffect
import com.dc.melodiasmario.feature.currenttrack.presentation.CurrentTrackEvent
import com.dc.melodiasmario.feature.currenttrack.presentation.CurrentTrackViewModel
import org.koin.compose.viewmodel.koinViewModel

@Composable
fun CurrentTrackRoute(
    modifier: Modifier = Modifier,
    viewModel: CurrentTrackViewModel = koinViewModel(),
    guildId: String,
    onGuildClicked: () -> Unit,
    onProfileClicked: () -> Unit,
    toastHostState: MToastHostState,
) {
    val uiState by viewModel.uiState.collectAsState()
    val effectMessages = currentTrackEffectMessages()

    LaunchedEffect(guildId) {
        viewModel.onEvent(CurrentTrackEvent.LoadCurrentTrack(guildId))
    }

    LaunchedEffect(viewModel) {
        viewModel.effect.collect { effect ->
            when (effect) {
                CurrentTrackEffect.NavigateToGuildSelector -> onGuildClicked()
                CurrentTrackEffect.NavigateToProfile -> onProfileClicked()
                else -> effectMessages.toastDataFor(effect)?.let { toastHostState.showToast(it) }
            }
        }
    }

    CurrentTrackScreen(
        modifier = modifier,
        uiState = uiState,
        onEvent = viewModel::onEvent
    )
}
