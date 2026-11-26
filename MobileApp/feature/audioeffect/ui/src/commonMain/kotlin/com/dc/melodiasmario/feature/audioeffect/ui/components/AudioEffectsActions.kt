package com.dc.melodiasmario.feature.audioeffect.ui.components

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.Icon
import androidx.compose.material3.LocalContentColor
import androidx.compose.material3.LocalTextStyle
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeMode
import com.dc.melodiasmario.core.model.audioeffect.AudioEffectsSettings
import com.dc.melodiasmario.core.model.audioeffect.AudioEffectsSnapshot
import com.dc.melodiasmario.core.model.currenttrack.CurrentTrack
import com.dc.melodiasmario.core.model.currenttrack.Track
import com.dc.melodiasmario.feature.audioeffect.generated.resources.Res
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_advanced
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_advanced_hint
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_apply
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_apply_advanced
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_presets_note
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_reset
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_reset_hint
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_save
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_save_as_preset
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_sync
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_volume_note
import com.dc.melodiasmario.feature.audioeffect.generated.resources.ic_effect_check
import com.dc.melodiasmario.feature.audioeffect.generated.resources.ic_effect_chevron
import com.dc.melodiasmario.feature.audioeffect.generated.resources.ic_effect_plus
import com.dc.melodiasmario.feature.audioeffect.generated.resources.ic_effect_reset
import com.dc.melodiasmario.feature.audioeffect.generated.resources.ic_effect_save
import com.dc.melodiasmario.feature.audioeffect.generated.resources.ic_effect_wave
import com.dc.melodiasmario.feature.audioeffect.presentation.AudioEffectsEvent
import com.dc.melodiasmario.feature.audioeffect.presentation.state.AudioEffectsUiState
import org.jetbrains.compose.resources.painterResource
import org.jetbrains.compose.resources.stringResource

/** Renders draft submission, preset saving, reset and synchronization actions. */
@Composable
internal fun AudioEffectsActions(
    uiState: AudioEffectsUiState,
    onEvent: (event: AudioEffectsEvent) -> Unit,
    onSavePreset: () -> Unit
) {
    if (uiState.tab == 2) {
        Column(verticalArrangement = Arrangement.spacedBy(space = 12.dp)) {
            Button(
                onClick = onSavePreset,
                enabled = uiState.canEdit && uiState.snapshot?.canManagePresets == true,
                modifier = Modifier.fillMaxWidth()
            ) {
                Icon(painter = painterResource(resource = Res.drawable.ic_effect_plus), contentDescription = null,
                    modifier = Modifier.padding(end = 8.dp))
                MText(text = stringResource(resource = Res.string.audio_effects_save), color = LocalContentColor.current)
            }
            MText(
                text = stringResource(resource = Res.string.audio_effects_presets_note),
                color = MaterialTheme.colorScheme.onSurfaceVariant,
                fontSize = 10.sp,
                lineHeight = 15.sp,
                textAlign = TextAlign.Start
            )
        }
        return
    }

    if (uiState.tab == 1) {
        Column(verticalArrangement = Arrangement.spacedBy(space = 8.dp)) {
            Button(
                onClick = { onEvent(AudioEffectsEvent.Apply) },
                enabled = uiState.canEdit && uiState.dirty,
                modifier = Modifier.fillMaxWidth()
            ) {
                MText(text = stringResource(resource = Res.string.audio_effects_apply_advanced), color = LocalContentColor.current)
            }
            OutlinedButton(
                onClick = onSavePreset,
                enabled = uiState.canEdit && uiState.snapshot?.canManagePresets == true,
                modifier = Modifier.fillMaxWidth(),
                border = BorderStroke(width = 1.5.dp, color = MaterialTheme.colorScheme.onSurface.copy(alpha = .65f)),
                colors = ButtonDefaults.outlinedButtonColors(contentColor = MaterialTheme.colorScheme.onSurface)
            ) {
                Icon(painter = painterResource(resource = Res.drawable.ic_effect_save), contentDescription = null,
                    modifier = Modifier.padding(end = 8.dp))
                MText(text = stringResource(resource = Res.string.audio_effects_save_as_preset), color = LocalContentColor.current)
            }
        }
        return
    }

    val enabled = uiState.canEdit
    Column(verticalArrangement = Arrangement.spacedBy(space = 8.dp)) {
        Button(onClick = { onEvent(AudioEffectsEvent.Apply) }, enabled = enabled && uiState.dirty, modifier = Modifier.fillMaxWidth()) {
            Icon(
                painter = painterResource(resource = Res.drawable.ic_effect_check),
                contentDescription = null,
                modifier = Modifier.padding(end = 8.dp)
            )
            MText(
                text = stringResource(resource = Res.string.audio_effects_apply),
                style = LocalTextStyle.current,
                color = LocalContentColor.current,
                textAlign = LocalTextStyle.current.textAlign,
                lineHeight = LocalTextStyle.current.lineHeight
            )
        }
        OutlinedButton(onClick = { onEvent(AudioEffectsEvent.Reset) }, enabled = enabled, modifier = Modifier.fillMaxWidth()) {
            Icon(
                painter = painterResource(resource = Res.drawable.ic_effect_reset),
                contentDescription = null,
                modifier = Modifier.padding(end = 8.dp)
            )
            MText(
                text = stringResource(resource = Res.string.audio_effects_reset),
                style = LocalTextStyle.current,
                color = LocalContentColor.current,
                textAlign = LocalTextStyle.current.textAlign,
                lineHeight = LocalTextStyle.current.lineHeight
            )
        }
        MText(
            text = uiState.currentTrack?.volumePercent?.let { volumePercent ->
                stringResource(resource = Res.string.audio_effects_volume_note, formatArgs = arrayOf<Any>(volumePercent))
            } ?: stringResource(resource = Res.string.audio_effects_reset_hint),
            style = MaterialTheme.typography.bodySmall,
            color = LocalContentColor.current,
            textAlign = MaterialTheme.typography.bodySmall.textAlign,
            lineHeight = MaterialTheme.typography.bodySmall.lineHeight
        )
        OutlinedButton(onClick = { onEvent(AudioEffectsEvent.Refresh) }, enabled = !uiState.busy,
            modifier = Modifier.fillMaxWidth()) {
            Icon(
                painter = painterResource(resource = Res.drawable.ic_effect_reset),
                contentDescription = null,
                modifier = Modifier.padding(end = 8.dp)
            )
            MText(
                text = stringResource(resource = Res.string.audio_effects_sync),
                style = LocalTextStyle.current,
                color = LocalContentColor.current,
                textAlign = LocalTextStyle.current.textAlign,
                lineHeight = LocalTextStyle.current.lineHeight
            )
        }
        if (uiState.tab == 0) {
            OutlinedButton(
                onClick = { onEvent(AudioEffectsEvent.TabSelected(index = 1)) },
                modifier = Modifier.fillMaxWidth()
            ) {
                Row(modifier = Modifier.fillMaxWidth(), verticalAlignment = Alignment.CenterVertically) {
                    Icon(
                        painter = painterResource(resource = Res.drawable.ic_effect_wave),
                        contentDescription = null,
                        tint = MaterialTheme.colorScheme.secondary,
                        modifier = Modifier.padding(end = 8.dp)
                    )
                    Column(modifier = Modifier.weight(weight = 1f)) {
                        MText(text = stringResource(resource = Res.string.audio_effects_advanced),
                            color = LocalContentColor.current, textAlign = TextAlign.Start)
                        MText(text = stringResource(resource = Res.string.audio_effects_advanced_hint),
                            style = MaterialTheme.typography.bodySmall, textAlign = TextAlign.Start)
                    }
                    Icon(
                        painter = painterResource(resource = Res.drawable.ic_effect_chevron),
                        contentDescription = null,
                        tint = MaterialTheme.colorScheme.secondary
                    )
                }
            }
        }
    }
}

@Preview(showBackground = true)
@Composable
private fun AudioEffectsActionsPreview() {
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
        AudioEffectsActions(uiState = uiState, onEvent = {}, onSavePreset = {})
    }
}

@Preview(showBackground = true)
@Composable
private fun AudioEffectsAdvancedActionsPreview() {
    MelodiasMarioTheme(themeMode = MelodiasMarioThemeMode.Dark) {
        AudioEffectsActions(
            uiState = AudioEffectsUiState(
                snapshot = AudioEffectsSnapshot(settings = AudioEffectsSettings(), presets = emptyList(),
                    canManagePresets = true, page = 1, pages = 1),
                busy = false,
                synchronized = true,
                tab = 1
            ),
            onEvent = {},
            onSavePreset = {}
        )
    }
}
