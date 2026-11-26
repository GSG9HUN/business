package com.dc.melodiasmario.feature.audioeffect.ui.components

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.selection.selectable
import androidx.compose.foundation.selection.selectableGroup
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.MaterialTheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.semantics.Role
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeMode
import com.dc.melodiasmario.feature.audioeffect.generated.resources.Res
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_advanced_tab
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_presets
import com.dc.melodiasmario.feature.audioeffect.presentation.AudioEffectsEvent
import org.jetbrains.compose.resources.stringResource

/** Renders two equally sized tabs for advanced settings and saved presets. */
@Composable
internal fun AudioEffectsTabs(selectedTab: Int, onEvent: (event: AudioEffectsEvent) -> Unit) {
    Column {
        Row(
            modifier = Modifier.fillMaxWidth()
                .selectableGroup()
        ) {
            listOf(Res.string.audio_effects_advanced_tab, Res.string.audio_effects_presets)
                .forEachIndexed { tabIndex, label ->
                    val selected = selectedTab == tabIndex + 1
                    Column(
                        modifier = Modifier.weight(weight = 1f)
                            .selectable(
                                selected = selected,
                                role = Role.Tab,
                                onClick = { onEvent(AudioEffectsEvent.TabSelected(index = tabIndex + 1)) }
                            ),
                        horizontalAlignment = Alignment.CenterHorizontally
                    ) {
                        MText(
                            text = stringResource(resource = label),
                            modifier = Modifier.padding(vertical = 11.dp),
                            fontSize = 10.sp,
                            fontWeight = if (selected) FontWeight.SemiBold else FontWeight.Normal,
                            color = if (selected) {
                                MaterialTheme.colorScheme.onSurface
                            } else {
                                MaterialTheme.colorScheme.onSurfaceVariant
                            }
                        )
                        HorizontalDivider(
                            thickness = 2.dp,
                            color = if (selected) MaterialTheme.colorScheme.primary else Color.Transparent
                        )
                    }
                }
        }
        HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant)
    }
}

@Preview(showBackground = true)
@Composable
private fun AudioEffectsTabsPreview() {
    MelodiasMarioTheme(themeMode = MelodiasMarioThemeMode.Dark) {
        AudioEffectsTabs(selectedTab = 1, onEvent = {})
    }
}
