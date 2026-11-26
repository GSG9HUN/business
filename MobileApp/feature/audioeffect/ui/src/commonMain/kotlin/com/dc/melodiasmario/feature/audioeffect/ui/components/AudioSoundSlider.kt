package com.dc.melodiasmario.feature.audioeffect.ui.components

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Slider
import androidx.compose.material3.SliderDefaults
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.semantics.contentDescription
import androidx.compose.ui.semantics.semantics
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeMode
import com.dc.melodiasmario.feature.audioeffect.ui.formatter.audioDisplayValue

/** Uses the playback card's circular thumb and gap-free track for sound controls. */
@Composable
internal fun AudioSoundSlider(
    label: String,
    value: Float,
    range: ClosedFloatingPointRange<Float>,
    enabled: Boolean,
    onValueChange: (value: Float) -> Unit,
    modifier: Modifier = Modifier,
    activeColor: Color = MaterialTheme.colorScheme.primary
) {
    Slider(
        value = value.coerceIn(range = range),
        onValueChange = onValueChange,
        enabled = enabled,
        valueRange = range,
        colors = SliderDefaults.colors(activeTrackColor = activeColor, thumbColor = activeColor),
        modifier = modifier.semantics { contentDescription = label },
        track = { sliderState ->
            SliderDefaults.Track(
                sliderState = sliderState,
                colors = SliderDefaults.colors(activeTrackColor = activeColor, thumbColor = activeColor),
                enabled = enabled,
                modifier = Modifier.height(height = 4.dp),
                drawStopIndicator = null,
                thumbTrackGapSize = 0.dp
            )
        },
        thumb = {
            Box(modifier = Modifier.height(height = 48.dp), contentAlignment = Alignment.Center) {
                Box(
                    modifier = Modifier.size(size = 10.dp)
                        .background(
                            color = if (enabled) activeColor else MaterialTheme.colorScheme.onSurface.copy(alpha = .38f),
                            shape = CircleShape
                        )
                )
            }
        }
    )
}

/** Shows the label at left and its highlighted current value at right, above the slider. */
@Composable
internal fun AudioLabeledSlider(
    label: String,
    value: Float,
    range: ClosedFloatingPointRange<Float>,
    enabled: Boolean,
    onValueChange: (value: Float) -> Unit,
    unit: String = ""
) {
    Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
        MText(text = label, color = MaterialTheme.colorScheme.onSurface)
        MText(text = audioDisplayValue(value = value) + unit, color = MaterialTheme.colorScheme.secondary,
            style = MaterialTheme.typography.labelLarge)
    }
    AudioSoundSlider(label = label, value = value, range = range, enabled = enabled, onValueChange = onValueChange)
}

@Preview(showBackground = true)
@Composable
private fun AudioSoundSliderPreview() {
    MelodiasMarioTheme(themeMode = MelodiasMarioThemeMode.Dark) {
        var speed by remember { mutableStateOf(value = 1f) }
        AudioLabeledSlider(
            label = "Speed",
            value = speed,
            range = .5f..2f,
            enabled = true,
            unit = "×",
            onValueChange = { value -> speed = value }
        )
    }
}
