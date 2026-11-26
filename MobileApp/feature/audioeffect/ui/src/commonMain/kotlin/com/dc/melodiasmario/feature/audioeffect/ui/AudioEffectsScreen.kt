package com.dc.melodiasmario.feature.audioeffect.ui

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.material3.HorizontalDivider
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.tooling.preview.Preview
import com.dc.melodiasmario.core.commonui.components.MErrorScreen
import com.dc.melodiasmario.core.commonui.components.MLoadingScreen
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeMode
import com.dc.melodiasmario.core.commonui.topbar.SetTopBarConfig
import com.dc.melodiasmario.core.commonui.topbar.TopBarConfig
import com.dc.melodiasmario.core.commonui.topbar.TopBarNavigationIcon
import com.dc.melodiasmario.core.model.audioeffect.AudioEffectsSettings
import com.dc.melodiasmario.core.model.audioeffect.AudioEffectsSnapshot
import com.dc.melodiasmario.core.model.currenttrack.CurrentTrack
import com.dc.melodiasmario.core.model.currenttrack.Track
import com.dc.melodiasmario.feature.audioeffect.generated.resources.Res
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_advanced_title
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_back
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_failed
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_title
import com.dc.melodiasmario.feature.audioeffect.presentation.AudioEffectsEvent
import com.dc.melodiasmario.feature.audioeffect.presentation.state.AudioEffectsUiState
import com.dc.melodiasmario.feature.audioeffect.ui.components.AudioEffectsContent
import org.jetbrains.compose.resources.stringResource

/** Configures the shared top bar and renders loading, error or content using the CurrentTrack column pattern. */
@Composable
fun AudioEffectsScreen(
    uiState: AudioEffectsUiState,
    onEvent: (event: AudioEffectsEvent) -> Unit,
    modifier: Modifier = Modifier
) {
    SetTopBarConfig(
        config = TopBarConfig(
            title = stringResource(
                resource = if (uiState.tab == 0) Res.string.audio_effects_title else Res.string.audio_effects_advanced_title
            ),
            subTitle = uiState.currentTrack?.let { playback ->
                listOfNotNull(
                    playback.guildName.takeIf { guildName -> guildName.isNotBlank() },
                    playback.guildBotStatus?.connectedVoiceChannelName?.takeIf { channelName -> channelName.isNotBlank() }
                ).joinToString(separator = " · ").takeIf { subtitle -> subtitle.isNotBlank() }
            },
            navigationIcon = TopBarNavigationIcon.Back(
                onClick = { onEvent(AudioEffectsEvent.BackClicked) },
                contentDescription = stringResource(resource = Res.string.audio_effects_back)
            )
        )
    )

    Column(modifier = modifier.fillMaxSize()) {
        HorizontalDivider()
        when {
            !uiState.synchronized && uiState.busy -> MLoadingScreen(
                modifier = Modifier.fillMaxWidth()
                    .weight(weight = 1f)
            )

            !uiState.synchronized && uiState.loadFailed -> MErrorScreen(
                modifier = Modifier.fillMaxWidth()
                    .weight(weight = 1f),
                errorMessage = stringResource(resource = Res.string.audio_effects_failed),
                onClick = { onEvent(AudioEffectsEvent.Refresh) }
            )

            else -> AudioEffectsContent(
                uiState = uiState,
                onEvent = onEvent,
                modifier = Modifier.fillMaxWidth()
                    .weight(weight = 1f)
            )
        }
    }
}

@Preview(showBackground = true)
@Composable
private fun AudioEffectsScreenPreview() {
    MelodiasMarioTheme(themeMode = MelodiasMarioThemeMode.Dark) {
        val uiState = AudioEffectsUiState(
            snapshot = AudioEffectsSnapshot(
                settings = AudioEffectsSettings(),
                presets = listOf("Late Night", "Warm & Soft"),
                canManagePresets = true,
                page = 1,
                pages = 1
            ),
            busy = false,
            synchronized = true,
            currentTrack = CurrentTrack(
                currentTrack = Track(title = "Nightcall", artist = "Kavinsky"),
                isPlaying = true,
                volumePercent = 85
            ),
            isConnected = true
        )
        AudioEffectsScreen(uiState = uiState, onEvent = {})
    }
}

@Preview(showBackground = true)
@Composable
private fun AudioEffectsLoadingPreview() {
    MelodiasMarioTheme(themeMode = MelodiasMarioThemeMode.Dark) {
        AudioEffectsScreen(uiState = AudioEffectsUiState(), onEvent = {})
    }
}

@Preview(showBackground = true)
@Composable
private fun AudioEffectsErrorPreview() {
    MelodiasMarioTheme(themeMode = MelodiasMarioThemeMode.Dark) {
        AudioEffectsScreen(uiState = AudioEffectsUiState(busy = false, loadFailed = true), onEvent = {})
    }
}
