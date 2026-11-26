package com.dc.melodiasmario.feature.audioeffect.ui

import androidx.compose.runtime.Composable
import androidx.compose.runtime.DisposableEffect
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import com.dc.melodiasmario.core.commonui.feedback.state.MToastHostState
import com.dc.melodiasmario.feature.audioeffect.presentation.AudioEffectsEffect
import com.dc.melodiasmario.feature.audioeffect.presentation.AudioEffectsViewModel
import kotlinx.coroutines.flow.collectLatest
import org.koin.compose.viewmodel.koinViewModel

/** Connects a guild-scoped editor to its ViewModel and terminal feedback for a Navigation3 destination. */
@Composable
fun AudioEffectsRoute(
    guildId: String,
    onBack: () -> Unit,
    toastHostState: MToastHostState,
    modifier: Modifier = Modifier,
    audioEffectsViewModel: AudioEffectsViewModel = koinViewModel()
) {
    val uiState by audioEffectsViewModel.uiState.collectAsState()
    val effectMessages = audioEffectsEffectMessages()

    DisposableEffect(key1 = guildId, key2 = audioEffectsViewModel) {
        audioEffectsViewModel.open(guildId = guildId)
        onDispose { audioEffectsViewModel.close() }
    }

    LaunchedEffect(key1 = audioEffectsViewModel) {
        audioEffectsViewModel.effect.collectLatest { effect ->
            when (effect) {
                AudioEffectsEffect.NavigateBack -> onBack()
                else -> effectMessages.toastDataFor(effect = effect)?.let { toastData ->
                    toastHostState.showToast(toast = toastData)
                }
            }
        }
    }

    AudioEffectsScreen(
        uiState = uiState,
        onEvent = audioEffectsViewModel::onEvent,
        modifier = modifier
    )
}
