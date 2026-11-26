package com.dc.melodiasmario.feature.audioeffect.ui.components

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.material3.LinearProgressIndicator
import androidx.compose.material3.LocalContentColor
import androidx.compose.material3.LocalTextStyle
import androidx.compose.material3.MaterialTheme
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeMode
import com.dc.melodiasmario.core.model.audioeffect.AudioEffectsSettings
import com.dc.melodiasmario.core.model.audioeffect.AudioEffectsSnapshot
import com.dc.melodiasmario.core.model.currenttrack.CurrentTrack
import com.dc.melodiasmario.core.model.currenttrack.Track
import com.dc.melodiasmario.feature.audioeffect.generated.resources.Res
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_failed
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_pending
import com.dc.melodiasmario.feature.audioeffect.presentation.AudioEffectsEvent
import com.dc.melodiasmario.feature.audioeffect.presentation.state.AudioEffectsUiState
import org.jetbrains.compose.resources.stringResource

/** Composes the editor content from focused sections and transient preset dialogs. */
@Composable
internal fun AudioEffectsContent(
    uiState: AudioEffectsUiState,
    onEvent: (event: AudioEffectsEvent) -> Unit,
    modifier: Modifier = Modifier
) {
    var showSaveDialog by remember { mutableStateOf(value = false) }
    var deletePreset by remember { mutableStateOf<String?>(value = null) }

    LazyColumn(
        modifier = modifier,
        contentPadding = PaddingValues(all = 20.dp),
        verticalArrangement = Arrangement.spacedBy(space = 16.dp)
    ) {
        item { AudioEffectsCurrentTrack(uiState = uiState, onEvent = onEvent) }
        item { AudioEffectsIntro(isAdvanced = uiState.tab != 0) }

        if (uiState.tab != 0) {
            item { AudioEffectsTabs(selectedTab = uiState.tab, onEvent = onEvent) }
        }

        if (uiState.busy) {
            item {
                LinearProgressIndicator(modifier = Modifier.fillMaxWidth())
                MText(
                    text = stringResource(resource = Res.string.audio_effects_pending),
                    style = MaterialTheme.typography.bodySmall,
                    color = LocalContentColor.current,
                    textAlign = MaterialTheme.typography.bodySmall.textAlign,
                    lineHeight = MaterialTheme.typography.bodySmall.lineHeight
                )
            }
        }

        if (uiState.loadFailed) {
            item {
                MText(
                    text = stringResource(resource = Res.string.audio_effects_failed),
                    color = MaterialTheme.colorScheme.error,
                    style = LocalTextStyle.current,
                    textAlign = LocalTextStyle.current.textAlign,
                    lineHeight = LocalTextStyle.current.lineHeight
                )
            }
        }


        when (uiState.tab) {
            0 -> audioEffectsBasicSection(uiState = uiState, onEvent = onEvent)
            1 -> audioEffectsAdvancedSection(uiState = uiState, onEvent = onEvent)
            2 -> audioEffectsPresetsSection(
                uiState = uiState,
                onEvent = onEvent,
                onDeletePreset = { presetName -> deletePreset = presetName }
            )
        }

        item {
            AudioEffectsActions(uiState = uiState, onEvent = onEvent, onSavePreset = { showSaveDialog = true })
        }
    }

    AudioEffectsDialogs(
        showSaveDialog = showSaveDialog,
        deletePreset = deletePreset,
        onEvent = onEvent,
        onSaveDismissed = { showSaveDialog = false },
        onDeleteDismissed = { deletePreset = null }
    )
}

@Preview(showBackground = true)
@Composable
private fun AudioEffectsContentPreview() {
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
        AudioEffectsContent(uiState = uiState, onEvent = {})
    }
}
