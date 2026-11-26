package com.dc.melodiasmario.feature.audioeffect.ui.components

import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Button
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.Icon
import androidx.compose.material3.LocalContentColor
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.semantics.Role
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeMode
import com.dc.melodiasmario.feature.audioeffect.generated.resources.Res
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_cancel
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_filter_apply_hint
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_filter_default
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_filter_edited
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_filter_keep
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_filter_preview
import com.dc.melodiasmario.feature.audioeffect.generated.resources.ic_effect_chevron
import com.dc.melodiasmario.feature.audioeffect.presentation.AudioEffectsEvent
import com.dc.melodiasmario.feature.audioeffect.presentation.model.AudioEffectControl
import com.dc.melodiasmario.feature.audioeffect.presentation.state.AudioEffectsUiState
import com.dc.melodiasmario.feature.audioeffect.ui.formatter.audioDisplayValue
import com.dc.melodiasmario.feature.audioeffect.ui.mapper.audioLabelResource
import kotlin.math.round
import org.jetbrains.compose.resources.painterResource
import org.jetbrains.compose.resources.stringResource

/** Groups advanced filters as popup rows; keeping an edit changes only the local draft. */
@Composable
internal fun AudioEffectsFilters(uiState: AudioEffectsUiState, onEvent: (event: AudioEffectsEvent) -> Unit) {
    var editingControl by remember { mutableStateOf<AudioEffectControl?>(value = null) }
    Surface(color = MaterialTheme.colorScheme.surfaceContainer, shape = MaterialTheme.shapes.large) {
        Column(modifier = Modifier.padding(horizontal = 16.dp)) {
            AudioEffectControl.entries.forEachIndexed { index, control ->
                val label = stringResource(resource = control.audioLabelResource())
                val title = label.substringBefore(delimiter = " · ")
                val values = uiState.draft.effects[control.filterName]
                Row(
                    modifier = Modifier.fillMaxWidth()
                        .clickable(enabled = uiState.canEdit, role = Role.Button, onClick = { editingControl = control })
                        .padding(vertical = 15.dp),
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Column(modifier = Modifier.weight(weight = 1f), verticalArrangement = Arrangement.spacedBy(space = 4.dp)) {
                        MText(
                            text = title, color = MaterialTheme.colorScheme.onSurface, fontSize = 11.sp,
                            fontWeight = FontWeight.Bold, textAlign = TextAlign.Start
                        )
                        MText(
                            text = if (values == null) {
                                stringResource(resource = Res.string.audio_effects_filter_default)
                            } else {
                                stringResource(
                                    resource = Res.string.audio_effects_filter_edited,
                                    formatArgs = arrayOf<Any>(audioDisplayValue(value = values[control.primaryIndex]))
                                )
                            }, fontSize = 10.sp, color = MaterialTheme.colorScheme.onSurfaceVariant, textAlign = TextAlign.Start
                        )
                    }
                    Icon(
                        painter = painterResource(resource = Res.drawable.ic_effect_chevron), contentDescription = null,
                        tint = MaterialTheme.colorScheme.onSurfaceVariant, modifier = Modifier.size(size = 18.dp)
                    )
                }

                if (index != AudioEffectControl.entries.lastIndex) {
                    HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant)
                }
            }
        }
    }

    editingControl?.let { control ->
        AudioEffectEditDialog(
            control = control,
            values = uiState.draft.effects[control.filterName] ?: control.defaults,
            onDismiss = { editingControl = null },
            onKeep = { value ->
                onEvent(AudioEffectsEvent.EffectChanged(control = control, value = value))
                editingControl = null
            }
        )
    }
}

/** Edits one primary filter parameter without sending a bot command before Apply. */
@Composable
private fun AudioEffectEditDialog(
    control: AudioEffectControl,
    values: List<Float>,
    onDismiss: () -> Unit,
    onKeep: (value: Float) -> Unit
) {
    var value by remember(key1 = control) { mutableStateOf(value = values[control.primaryIndex]) }
    val label = stringResource(resource = control.audioLabelResource())
    AlertDialog(
        onDismissRequest = onDismiss,
        title = { MText(text = label.substringBefore(delimiter = " · "), color = MaterialTheme.colorScheme.onSurface) },
        text = {
            Column(verticalArrangement = Arrangement.spacedBy(space = 16.dp)) {
                MText(
                    text = stringResource(resource = Res.string.audio_effects_filter_preview), fontSize = 11.sp,
                    textAlign = TextAlign.Start
                )
                AudioLabeledSlider(
                    label = label.substringAfter(delimiter = " · "), value = value,
                    range = control.minimum..control.maximum, enabled = true,
                    onValueChange = { newValue -> value = round(x = newValue * 100f) / 100f })
                MText(
                    text = stringResource(resource = Res.string.audio_effects_filter_apply_hint), fontSize = 10.sp,
                    textAlign = TextAlign.Start
                )
            }
        },
        confirmButton = {
            Button(onClick = { onKeep(value) }) {
                MText(text = stringResource(resource = Res.string.audio_effects_filter_keep), color = LocalContentColor.current)
            }
        },
        dismissButton = {
            TextButton(onClick = onDismiss) {
                MText(text = stringResource(resource = Res.string.audio_effects_cancel), color = LocalContentColor.current)
            }
        }
    )
}

@Preview(showBackground = true)
@Composable
private fun AudioEffectsFiltersPreview() {
    MelodiasMarioTheme(themeMode = MelodiasMarioThemeMode.Dark) {
        AudioEffectsFilters(uiState = AudioEffectsUiState(busy = false, synchronized = true), onEvent = {})
    }
}

@Preview(showBackground = true)
@Composable
private fun AudioEffectEditDialogPreview() {
    MelodiasMarioTheme(themeMode = MelodiasMarioThemeMode.Dark) {
        AudioEffectEditDialog(
            control = AudioEffectControl.Karaoke, values = AudioEffectControl.Karaoke.defaults,
            onDismiss = {}, onKeep = {})
    }
}
