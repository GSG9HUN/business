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

/**
 * Connects current track presentation state and effects to its screen callbacks.
 */
@Composable
fun CurrentTrackRoute(
    modifier: Modifier = Modifier,
    currentTrackViewModel: CurrentTrackViewModel = koinViewModel(),
    guildId: String,
    onGuildClicked: () -> Unit,
    onProfileClicked: () -> Unit,
    toastHostState: MToastHostState
): Unit {
    val uiState by currentTrackViewModel.uiState.collectAsState()
    val effectMessages = currentTrackEffectMessages()

    LaunchedEffect(key1 = guildId) {
        currentTrackViewModel.onEvent(event = CurrentTrackEvent.LoadCurrentTrack(guildId = guildId))
    }

    LaunchedEffect(key1 = currentTrackViewModel) {
        currentTrackViewModel.effect.collect { effect ->
            when (effect) {
                CurrentTrackEffect.NavigateToGuildSelector -> onGuildClicked()
                CurrentTrackEffect.NavigateToProfile -> onProfileClicked()
                else -> effectMessages.toastDataFor(effect = effect)
                    ?.let { toastData -> toastHostState.showToast(toast = toastData) }
            }
        }
    }

    CurrentTrackScreen(
        modifier = modifier,
        uiState = uiState,
        onEvent = currentTrackViewModel::onEvent
    )
}
