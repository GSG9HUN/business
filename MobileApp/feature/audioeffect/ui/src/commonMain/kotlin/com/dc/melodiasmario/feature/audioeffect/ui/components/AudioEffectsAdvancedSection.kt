package com.dc.melodiasmario.feature.audioeffect.ui.components

import androidx.compose.foundation.horizontalScroll
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.requiredWidth
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.LazyListScope
import androidx.compose.foundation.rememberScrollState
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.rotate
import androidx.compose.ui.text.font.FontWeight
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
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_eq_band
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_eq_caption
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_eq_scroll
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_equalizer
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_reset_eq
import com.dc.melodiasmario.feature.audioeffect.presentation.AudioEffectsEvent
import com.dc.melodiasmario.feature.audioeffect.presentation.state.AudioEffectsUiState
import com.dc.melodiasmario.feature.audioeffect.ui.formatter.audioDisplayValue
import kotlin.math.round
import org.jetbrains.compose.resources.stringResource

/** Renders the equalizer panel and the grouped popup filter editor. */
internal fun LazyListScope.audioEffectsAdvancedSection(
    uiState: AudioEffectsUiState,
    onEvent: (event: AudioEffectsEvent) -> Unit
) {
    item {
        val scrollState = rememberScrollState()
        val cyan = MaterialTheme.colorScheme.secondary
        val muted = MaterialTheme.colorScheme.onSurfaceVariant
        Surface(color = MaterialTheme.colorScheme.surfaceContainer, shape = MaterialTheme.shapes.large) {
            Column(modifier = Modifier.padding(all = 16.dp), verticalArrangement = Arrangement.spacedBy(space = 8.dp)) {
                Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically) {
                    MText(text = stringResource(resource = Res.string.audio_effects_equalizer),
                        color = MaterialTheme.colorScheme.onSurface, fontSize = 11.sp, fontWeight = FontWeight.SemiBold)
                    TextButton(onClick = { onEvent(AudioEffectsEvent.ResetEqualizer) }, enabled = uiState.canEdit) {
                        MText(text = stringResource(resource = Res.string.audio_effects_reset_eq), color = cyan, fontSize = 10.sp)
                    }
                }
                MText(text = stringResource(resource = Res.string.audio_effects_eq_caption), color = muted,
                    fontSize = 9.sp, textAlign = TextAlign.Start)
                Row(modifier = Modifier.horizontalScroll(state = scrollState),
                    horizontalArrangement = Arrangement.spacedBy(space = 8.dp)) {
                    val frequencies = listOf("25", "40", "63", "100", "160", "250", "400", "630", "1k", "1.6k", "2.5k", "4k", "6.3k", "10k", "16k")
                    frequencies.forEachIndexed { index, frequency ->
                        Column(modifier = Modifier.width(width = 40.dp), horizontalAlignment = Alignment.CenterHorizontally) {
                            MText(text = audioDisplayValue(value = uiState.draft.equalizer[index]), color = cyan, fontSize = 8.sp)
                            Box(modifier = Modifier.height(height = 132.dp), contentAlignment = Alignment.Center) {
                                AudioSoundSlider(
                                    label = stringResource(
                                        resource = Res.string.audio_effects_eq_band,
                                        formatArgs = arrayOf<Any>(frequency)
                                    ),
                                    value = uiState.draft.equalizer[index],
                                    range = -0.25f..0.25f,
                                    enabled = uiState.canEdit,
                                    modifier = Modifier.requiredWidth(width = 110.dp).rotate(degrees = -90f),
                                    activeColor = cyan,
                                    onValueChange = { gain ->
                                        onEvent(AudioEffectsEvent.EqualizerChanged(index = index, gain = round(x = gain * 20f) / 20f))
                                    }
                                )
                            }
                            MText(text = frequency, color = muted, fontSize = 8.sp)
                        }
                    }
                }
                AudioEffectsScrollBar(scrollState = scrollState)
                MText(text = stringResource(resource = Res.string.audio_effects_eq_scroll), color = muted,
                    fontSize = 9.sp, textAlign = TextAlign.Start)
            }
        }
    }
    item { AudioEffectsFilters(uiState = uiState, onEvent = onEvent) }
}

@Preview(showBackground = true)
@Composable
private fun AudioEffectsAdvancedSectionPreview() {
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
            audioEffectsAdvancedSection(uiState = uiState, onEvent = {})
        }
    }
}
