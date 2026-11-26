package com.dc.melodiasmario.feature.audioeffect.ui.components

import androidx.compose.foundation.horizontalScroll
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.LazyListScope
import androidx.compose.foundation.rememberScrollState
import androidx.compose.material3.FilterChip
import androidx.compose.material3.LocalContentColor
import androidx.compose.material3.LocalTextStyle
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
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
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_bass
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_high
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_low
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_medium
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_off
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_pitch
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_speed
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_tuning
import com.dc.melodiasmario.feature.audioeffect.presentation.AudioEffectsEvent
import com.dc.melodiasmario.feature.audioeffect.presentation.state.AudioEffectsUiState
import kotlin.math.abs
import kotlin.math.round
import org.jetbrains.compose.resources.stringResource

/** Adds the basic editor section while forwarding user intentions. */
internal fun LazyListScope.audioEffectsBasicSection(uiState: AudioEffectsUiState, onEvent: (event: AudioEffectsEvent) -> Unit) {
    val enabled = uiState.canEdit
    item { AudioEffectsProfiles(uiState = uiState, onEvent = onEvent) }

    item {
        MText(
            text = stringResource(resource = Res.string.audio_effects_tuning),
            style = MaterialTheme.typography.titleMedium,
            color = LocalContentColor.current,
            textAlign = MaterialTheme.typography.titleMedium.textAlign,
            lineHeight = MaterialTheme.typography.titleMedium.lineHeight
        )
    }
    item {
        Surface(
            color = MaterialTheme.colorScheme.surfaceContainer,
            shape = MaterialTheme.shapes.large,
            modifier = Modifier.fillMaxWidth()
        ) {
            Column(
                modifier = Modifier.padding(all = 16.dp),
                verticalArrangement = Arrangement.spacedBy(space = 8.dp)
            ) {
                MText(
                    text = stringResource(resource = Res.string.audio_effects_bass),
                    style = LocalTextStyle.current,
                    color = LocalContentColor.current,
                    textAlign = LocalTextStyle.current.textAlign,
                    lineHeight = LocalTextStyle.current.lineHeight
                )
                Row(modifier = Modifier.horizontalScroll(state = rememberScrollState())) {
                    val labels = listOf(Res.string.audio_effects_off, Res.string.audio_effects_low,
                        Res.string.audio_effects_medium, Res.string.audio_effects_high)
                    listOf(0f, .10f, .18f, .25f).forEachIndexed { index, gain ->
                        val factors = listOf(.6f, 1f, 1f, .8f, .4f, .15f)
                        val selected = uiState.draft.equalizer.withIndex().all { (band, value) ->
                            abs(x = value - (factors.getOrNull(index = band) ?: 0f) * gain) < .0001f
                        }
                        FilterChip(
                            selected = selected,
                            onClick = { onEvent(AudioEffectsEvent.BassChanged(gain = gain)) },
                            enabled = enabled,
                            label = {
                                MText(
                                    text = stringResource(resource = labels[index]),
                                    style = LocalTextStyle.current,
                                    color = LocalContentColor.current,
                                    textAlign = LocalTextStyle.current.textAlign,
                                    lineHeight = LocalTextStyle.current.lineHeight
                                )
                            }
                        )
                    }
                }
                AudioLabeledSlider(
                    label = stringResource(resource = Res.string.audio_effects_speed),
                    value = uiState.draft.speed,
                    range = .5f..2f,
                    unit = "×",
                    enabled = enabled,
                    onValueChange = { value -> onEvent(AudioEffectsEvent.SpeedChanged(value = round(x = value * 20f) / 20f)) }
                )
                AudioLabeledSlider(
                    label = stringResource(resource = Res.string.audio_effects_pitch),
                    value = uiState.draft.pitch,
                    range = .5f..2f,
                    unit = "×",
                    enabled = enabled,
                    onValueChange = { value -> onEvent(AudioEffectsEvent.PitchChanged(value = round(x = value * 20f) / 20f)) }
                )

            }
        }
    }
}

@Preview(showBackground = true)
@Composable
private fun AudioEffectsBasicSectionPreview() {
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
            audioEffectsBasicSection(uiState = uiState, onEvent = {})
        }
    }
}
