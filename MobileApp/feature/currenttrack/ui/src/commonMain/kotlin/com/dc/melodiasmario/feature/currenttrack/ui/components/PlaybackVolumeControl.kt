package com.dc.melodiasmario.feature.currenttrack.ui.components

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedIconButton
import androidx.compose.material3.Slider
import androidx.compose.material3.SliderDefaults
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.key
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.semantics.contentDescription
import androidx.compose.ui.semantics.semantics
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.feature.currenttrack.generated.resources.Res
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_volume_amplified
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_volume_confirmed
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_volume_decrease
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_volume_increase
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_volume_label
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_volume_maximum
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_volume_minus
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_volume_pending
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_volume_percent
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_volume_plus
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_volume_select_hint
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_volume_unavailable
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_volume_unknown_value
import kotlin.math.roundToInt
import org.jetbrains.compose.resources.stringResource

/**
 * Displays confirmed guild volume with a local drag preview and five-percent step buttons.
 * A completed gesture submits once; pending changes never replace the confirmed value locally.
 * Unknown confirmed volume permits absolute selection; relative steps require a valid confirmed value.
 */
@Composable
internal fun PlaybackVolumeControl(
    guildId: String,
    confirmedVolumePercent: Int?,
    isEnabled: Boolean,
    isLoading: Boolean,
    onVolumeRequested: (guildId: String, volumePercent: Int) -> Unit,
    modifier: Modifier = Modifier
) {
    val hasValidVolume = confirmedVolumePercent != null && confirmedVolumePercent in 0..200
    val canChangeVolume = isEnabled && !isLoading
    val canStepVolume = canChangeVolume && hasValidVolume
    var previewVolumePercent by remember(key1 = guildId) { mutableStateOf<Float?>(value = null) }

    LaunchedEffect(key1 = canChangeVolume) {
        if (!canChangeVolume) {
            previewVolumePercent = null
        }
    }

    val visibleVolumePercent = previewVolumePercent?.roundToInt() ?: confirmedVolumePercent
    val volumeLabel = stringResource(resource = Res.string.currenttrack_volume_label)
    val decreaseLabel = stringResource(resource = Res.string.currenttrack_volume_decrease)
    val increaseLabel = stringResource(resource = Res.string.currenttrack_volume_increase)
    val submitVolume: (volumePercent: Int) -> Unit = { volumePercent ->
        previewVolumePercent = null

        if (canChangeVolume && volumePercent != confirmedVolumePercent) {
            onVolumeRequested(guildId, volumePercent.coerceIn(minimumValue = 0, maximumValue = 200))
        }
    }
    val sliderColors = SliderDefaults.colors()

    Column(modifier = modifier) {
        Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
            MText(text = volumeLabel, fontSize = 12.sp)
            MText(
                text = if (previewVolumePercent != null || hasValidVolume) {
                    stringResource(
                        resource = Res.string.currenttrack_volume_percent,
                        formatArgs = arrayOf<Any>(requireNotNull(value = visibleVolumePercent))
                    )
                } else {
                    stringResource(resource = Res.string.currenttrack_volume_unknown_value)
                },
                fontSize = 12.sp
            )
        }
        Row(modifier = Modifier.fillMaxWidth(), verticalAlignment = Alignment.CenterVertically) {
            OutlinedIconButton(
                onClick = { submitVolume((confirmedVolumePercent ?: 0) - 5) },
                enabled = canStepVolume && requireNotNull(value = confirmedVolumePercent) > 0,
                modifier = Modifier.semantics { contentDescription = decreaseLabel }
            ) {
                MText(text = stringResource(resource = Res.string.currenttrack_volume_minus))
            }
            key(guildId) {
                Slider(
                    value = (previewVolumePercent ?: confirmedVolumePercent?.toFloat() ?: 0f)
                        .coerceIn(minimumValue = 0f, maximumValue = 200f),
                    onValueChange = { volumePercent -> previewVolumePercent = volumePercent },
                    modifier = Modifier.weight(weight = 1f)
                        .semantics { contentDescription = volumeLabel },
                    enabled = canChangeVolume,
                    valueRange = 0f..200f,
                    colors = sliderColors,
                    track = { sliderState ->
                        SliderDefaults.Track(
                            sliderState = sliderState,
                            modifier = Modifier.height(height = 4.dp),
                            colors = sliderColors,
                            enabled = canChangeVolume,
                            drawStopIndicator = null,
                            thumbTrackGapSize = 0.dp
                        )
                    },
                    thumb = { _ ->
                        Box(modifier = Modifier.height(height = 48.dp), contentAlignment = Alignment.Center) {
                            Box(
                                modifier = Modifier.size(size = 10.dp)
                                    .background(
                                        color = if (canChangeVolume) {
                                            MaterialTheme.colorScheme.primary
                                        } else {
                                            MaterialTheme.colorScheme.onSurface.copy(alpha = 0.38f)
                                        },
                                        shape = CircleShape
                                    )
                            )
                        }
                    },
                    onValueChangeFinished = {
                        val selectedVolumePercent = (previewVolumePercent ?: confirmedVolumePercent?.toFloat() ?: 0f).roundToInt()
                            .coerceIn(minimumValue = 0, maximumValue = 200)
                        submitVolume(selectedVolumePercent)
                    }
                )
            }
            OutlinedIconButton(
                onClick = { submitVolume((confirmedVolumePercent ?: 0) + 5) },
                enabled = canStepVolume && requireNotNull(value = confirmedVolumePercent) < 200,
                modifier = Modifier.semantics { contentDescription = increaseLabel }
            ) {
                MText(text = stringResource(resource = Res.string.currenttrack_volume_plus))
            }
        }
        Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
            MText(
                text = when {
                    isLoading -> stringResource(resource = Res.string.currenttrack_volume_pending)
                    !isEnabled -> stringResource(resource = Res.string.currenttrack_volume_unavailable)
                    (visibleVolumePercent ?: 0) > 100 -> stringResource(resource = Res.string.currenttrack_volume_amplified)
                    !hasValidVolume -> stringResource(resource = Res.string.currenttrack_volume_select_hint)
                    else -> stringResource(
                        resource = Res.string.currenttrack_volume_confirmed,
                        formatArgs = arrayOf<Any>(requireNotNull(value = confirmedVolumePercent))
                    )
                },
                fontSize = 10.sp
            )
            MText(text = stringResource(resource = Res.string.currenttrack_volume_maximum), fontSize = 10.sp)
        }
    }
}

/** Shows the volume control without a live repository or audio connection. */
@Preview
@Composable
private fun PlaybackVolumeControlPreview() {
    PlaybackVolumeControl(
        guildId = "preview",
        confirmedVolumePercent = 85,
        isEnabled = true,
        isLoading = false,
        onVolumeRequested = { _, _ -> }
    )
}
