package com.dc.melodiasmario.feature.audioeffect.ui.components

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.LazyListScope
import androidx.compose.foundation.lazy.items
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.LocalContentColor
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Surface
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
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
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_apply_preset
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_delete
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_empty
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_next
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_page
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_preset_subtitle
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_presets_callout
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_previous
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_server_fallback
import com.dc.melodiasmario.feature.audioeffect.generated.resources.ic_effect_server
import com.dc.melodiasmario.feature.audioeffect.generated.resources.ic_effect_trash
import com.dc.melodiasmario.feature.audioeffect.generated.resources.ic_effect_wave
import com.dc.melodiasmario.feature.audioeffect.presentation.AudioEffectsEvent
import com.dc.melodiasmario.feature.audioeffect.presentation.state.AudioEffectsUiState
import org.jetbrains.compose.resources.painterResource
import org.jetbrains.compose.resources.stringResource

/** Shows the server callout, compact saved-preset rows and optional page navigation. */
internal fun LazyListScope.audioEffectsPresetsSection(
    uiState: AudioEffectsUiState,
    onEvent: (event: AudioEffectsEvent) -> Unit,
    onDeletePreset: (name: String) -> Unit
) {
    item {
        Surface(
            color = MaterialTheme.colorScheme.primaryContainer.copy(alpha = .35f),
            shape = MaterialTheme.shapes.medium,
            border = BorderStroke(width = 1.dp, color = MaterialTheme.colorScheme.primary.copy(alpha = .25f))
        ) {
            Row(
                modifier = Modifier.fillMaxWidth().padding(all = 12.dp),
                horizontalArrangement = Arrangement.spacedBy(space = 9.dp), verticalAlignment = Alignment.CenterVertically
            ) {
                Icon(
                    painter = painterResource(resource = Res.drawable.ic_effect_server), contentDescription = null,
                    tint = MaterialTheme.colorScheme.secondary, modifier = Modifier.size(size = 18.dp)
                )
                val guildName = uiState.currentTrack?.guildName?.takeIf { name -> name.isNotBlank() }
                    ?: stringResource(resource = Res.string.audio_effects_server_fallback)
                val message =
                    stringResource(resource = Res.string.audio_effects_presets_callout, formatArgs = arrayOf<Any>(guildName))
                MText(
                    text = message, color = MaterialTheme.colorScheme.onSurface,
                    fontSize = 10.sp, lineHeight = 15.sp, textAlign = TextAlign.Start
                )
            }
        }
    }

    if (uiState.snapshot?.presets.isNullOrEmpty()) {
        item {
            MText(
                text = stringResource(resource = Res.string.audio_effects_empty),
                modifier = Modifier.fillMaxWidth().padding(vertical = 16.dp), color = MaterialTheme.colorScheme.onSurfaceVariant
            )
        }
    }

    items(items = uiState.snapshot?.presets.orEmpty(), key = { presetName -> presetName }) { presetName ->
        Column {
            Row(
                modifier = Modifier.fillMaxWidth().padding(vertical = 8.dp),
                horizontalArrangement = Arrangement.spacedBy(space = 8.dp), verticalAlignment = Alignment.CenterVertically
            ) {
                Surface(color = MaterialTheme.colorScheme.secondaryContainer, shape = MaterialTheme.shapes.small) {
                    Icon(
                        painter = painterResource(resource = Res.drawable.ic_effect_wave), contentDescription = null,
                        tint = MaterialTheme.colorScheme.secondary, modifier = Modifier.padding(all = 9.dp).size(size = 18.dp)
                    )
                }
                Column(modifier = Modifier.weight(weight = 1f)) {
                    MText(
                        text = presetName,
                        color = MaterialTheme.colorScheme.onSurface,
                        fontSize = 11.sp,
                        fontWeight = FontWeight.SemiBold,
                        textAlign = TextAlign.Start,
                        maxLines = 1,
                        overflow = TextOverflow.Ellipsis
                    )
                    MText(
                        text = stringResource(resource = Res.string.audio_effects_preset_subtitle), fontSize = 9.sp,
                        textAlign = TextAlign.Start, lineHeight = 14.sp
                    )
                }
                OutlinedButton(
                    onClick = { onEvent(AudioEffectsEvent.PresetSelected(name = presetName, custom = true)) },
                    enabled = uiState.canEdit,
                    modifier = Modifier.height(height = 32.dp),
                    contentPadding = PaddingValues(horizontal = 8.dp, vertical = 0.dp),
                    shape = MaterialTheme.shapes.small,
                    colors = ButtonDefaults.outlinedButtonColors(contentColor = MaterialTheme.colorScheme.onSurface)
                ) {
                    MText(
                        text = stringResource(resource = Res.string.audio_effects_apply_preset),
                        fontSize = 9.sp, color = LocalContentColor.current
                    )
                }
                IconButton(
                    onClick = { onDeletePreset(presetName) },
                    enabled = uiState.canEdit && uiState.snapshot?.canManagePresets == true
                ) {
                    Icon(
                        painter = painterResource(resource = Res.drawable.ic_effect_trash),
                        contentDescription = stringResource(resource = Res.string.audio_effects_delete),
                        tint = MaterialTheme.colorScheme.onSurfaceVariant, modifier = Modifier.size(size = 18.dp)
                    )
                }
            }
            HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant)
        }
    }

    if ((uiState.snapshot?.pages ?: 1) > 1) {
        item {
            Row(
                modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.CenterVertically
            ) {
                TextButton(
                    onClick = { onEvent(AudioEffectsEvent.PageSelected(page = (uiState.snapshot?.page ?: 1) - 1)) },
                    enabled = uiState.canEdit && (uiState.snapshot?.page ?: 1) > 1
                ) {
                    MText(text = stringResource(resource = Res.string.audio_effects_previous))
                }
                MText(
                    text = stringResource(
                        resource = Res.string.audio_effects_page,
                        formatArgs = arrayOf<Any>(uiState.snapshot?.page ?: 1, uiState.snapshot?.pages ?: 1)
                    )
                )
                TextButton(
                    onClick = { onEvent(AudioEffectsEvent.PageSelected(page = (uiState.snapshot?.page ?: 1) + 1)) },
                    enabled = uiState.canEdit && (uiState.snapshot?.page ?: 1) < (uiState.snapshot?.pages ?: 1)
                ) {
                    MText(text = stringResource(resource = Res.string.audio_effects_next))
                }
            }
        }
    }
}

@Preview(showBackground = true)
@Composable
private fun AudioEffectsPresetsSectionPreview() {
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
        LazyColumn {
            audioEffectsPresetsSection(uiState = uiState, onEvent = {}, onDeletePreset = {})
        }
    }
}
