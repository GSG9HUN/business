package com.dc.melodiasmario.feature.audioeffect.ui.components

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.heightIn
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.semantics.selected
import androidx.compose.ui.semantics.semantics
import androidx.compose.ui.text.style.TextAlign
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
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_bass_heavy
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_bass_heavy_hint
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_bass_light
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_bass_light_hint
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_custom
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_flat
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_flat_hint
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_nightcore
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_nightcore_hint
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_profiles
import com.dc.melodiasmario.feature.audioeffect.generated.resources.ic_effect_bolt
import com.dc.melodiasmario.feature.audioeffect.generated.resources.ic_effect_check
import com.dc.melodiasmario.feature.audioeffect.generated.resources.ic_effect_headphones
import com.dc.melodiasmario.feature.audioeffect.generated.resources.ic_effect_sparkles
import com.dc.melodiasmario.feature.audioeffect.generated.resources.ic_effect_wave
import com.dc.melodiasmario.feature.audioeffect.presentation.AudioEffectsEvent
import com.dc.melodiasmario.feature.audioeffect.presentation.mapper.profileName
import com.dc.melodiasmario.feature.audioeffect.presentation.state.AudioEffectsUiState
import org.jetbrains.compose.resources.painterResource
import org.jetbrains.compose.resources.stringResource

/** Shows the four sound profiles, their icons, confirmed selection and the green profile badge. */
@Composable
internal fun AudioEffectsProfiles(uiState: AudioEffectsUiState, onEvent: (event: AudioEffectsEvent) -> Unit) {
    val selectedName = uiState.snapshot?.settings?.profileName()
    val profileNames = listOf("flat", "bass-light", "bass-heavy", "nightcore")
    val titles = listOf(
        Res.string.audio_effects_flat, Res.string.audio_effects_bass_light,
        Res.string.audio_effects_bass_heavy, Res.string.audio_effects_nightcore
    )
    val descriptions = listOf(
        Res.string.audio_effects_flat_hint, Res.string.audio_effects_bass_light_hint,
        Res.string.audio_effects_bass_heavy_hint, Res.string.audio_effects_nightcore_hint
    )
    val icons = listOf(
        Res.drawable.ic_effect_headphones, Res.drawable.ic_effect_wave,
        Res.drawable.ic_effect_bolt, Res.drawable.ic_effect_sparkles
    )
    val selectedIndex = selectedName?.let { profileName -> profileNames.indexOf(element = profileName) } ?: -1

    Column(verticalArrangement = Arrangement.spacedBy(space = 12.dp)) {
        Row(
            modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween,
            verticalAlignment = Alignment.CenterVertically
        ) {
            MText(
                text = stringResource(resource = Res.string.audio_effects_profiles),
                style = MaterialTheme.typography.titleMedium, color = MaterialTheme.colorScheme.onSurface
            )
            Surface(color = MaterialTheme.colorScheme.tertiaryContainer, shape = MaterialTheme.shapes.small) {
                Row(
                    modifier = Modifier.padding(horizontal = 10.dp, vertical = 6.dp),
                    horizontalArrangement = Arrangement.spacedBy(space = 6.dp), verticalAlignment = Alignment.CenterVertically
                ) {
                    Icon(
                        painter = painterResource(resource = Res.drawable.ic_effect_check), contentDescription = null,
                        tint = MaterialTheme.colorScheme.tertiary, modifier = Modifier.size(size = 14.dp)
                    )
                    MText(
                        text = stringResource(resource = if (selectedIndex >= 0) titles[selectedIndex] else Res.string.audio_effects_custom),
                        color = MaterialTheme.colorScheme.tertiary, style = MaterialTheme.typography.labelMedium
                    )
                }
            }
        }
        profileNames.chunked(size = 2).forEach { profileRow ->
            Row(horizontalArrangement = Arrangement.spacedBy(space = 10.dp)) {
                profileRow.forEach { profileName ->
                    val index = profileNames.indexOf(element = profileName)
                    val selected = selectedName == profileName
                    OutlinedButton(
                        onClick = { onEvent(AudioEffectsEvent.PresetSelected(name = profileName)) },
                        enabled = uiState.canEdit,
                        modifier = Modifier.weight(weight = 1f).heightIn(min = 140.dp)
                            .semantics { this.selected = selected },
                        shape = MaterialTheme.shapes.large,
                        border = BorderStroke(
                            width = 1.dp,
                            color = if (selected) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.outlineVariant
                        ),
                        colors = ButtonDefaults.outlinedButtonColors(
                            containerColor = if (selected) MaterialTheme.colorScheme.primaryContainer else MaterialTheme.colorScheme.surfaceContainer,
                            contentColor = MaterialTheme.colorScheme.onSurface
                        ),
                        contentPadding = PaddingValues(all = 16.dp)
                    ) {
                        Column(modifier = Modifier.fillMaxWidth(), verticalArrangement = Arrangement.spacedBy(space = 10.dp)) {
                            Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                                Icon(
                                    painter = painterResource(resource = icons[index]), contentDescription = null,
                                    modifier = Modifier.size(size = 28.dp), tint = MaterialTheme.colorScheme.secondary
                                )

                                if (selected) {
                                    Icon(
                                        painter = painterResource(resource = Res.drawable.ic_effect_check),
                                        contentDescription = null,
                                        tint = MaterialTheme.colorScheme.secondary,
                                        modifier = Modifier.size(size = 18.dp)
                                    )
                                }
                            }
                            MText(
                                text = stringResource(resource = titles[index]), color = MaterialTheme.colorScheme.onSurface,
                                style = MaterialTheme.typography.titleMedium, textAlign = TextAlign.Start
                            )
                            MText(
                                text = stringResource(resource = descriptions[index]), style = MaterialTheme.typography.bodySmall,
                                textAlign = TextAlign.Start
                            )
                        }
                    }
                }
            }
        }
    }
}

@Preview(showBackground = true)
@Composable
private fun AudioEffectsProfilesPreview() {
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
        AudioEffectsProfiles(uiState = uiState, onEvent = {})
    }
}
