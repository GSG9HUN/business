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
import androidx.compose.material3.Slider
import androidx.compose.material3.SliderDefaults
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
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
import androidx.compose.ui.unit.sp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.model.currenttrack.CurrentTrack
import com.dc.melodiasmario.core.model.currenttrack.Track
import com.dc.melodiasmario.core.presentation.formatter.toDurationLabel
import com.dc.melodiasmario.feature.currenttrack.generated.resources.Res
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_seek_label
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_seek_pending
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_seek_unavailable
import kotlin.math.roundToInt
import kotlin.time.Duration.Companion.milliseconds
import kotlinx.coroutines.delay
import org.jetbrains.compose.resources.stringResource

/**
 * Displays a local position preview and submits one seek when a gesture finishes.
 * Attempt or guild changes discard the preview; server position remains authoritative.
 */
@Composable
internal fun PlaybackSeekBar(
    currentTrack: CurrentTrack,
    isPlaying: Boolean,
    isEnabled: Boolean,
    isLoading: Boolean,
    onSeekRequested: (positionSeconds: Int, expectedPlaybackOperationId: String) -> Unit,
    modifier: Modifier = Modifier
) {
    val track = currentTrack.currentTrack ?: return
    val playbackOperationId = track.playbackOperationId
    val canSeek = isEnabled && !isLoading && track.isSeekable
        && track.durationSeconds > 1 && !playbackOperationId.isNullOrBlank()
    var seekPreviewSeconds by remember(key1 = currentTrack.guildId, key2 = playbackOperationId) {
        mutableStateOf<Float?>(value = null)
    }
    var displayedPositionSeconds by remember(
        key1 = currentTrack.guildId,
        key2 = playbackOperationId,
        key3 = track.id
    ) {
        mutableStateOf(value = currentTrack.positionSeconds)
    }

    LaunchedEffect(
        currentTrack.guildId,
        playbackOperationId,
        track.id,
        currentTrack.positionSeconds,
        currentTrack.updatedAtUtc,
        isPlaying,
        isEnabled
    ) {
        displayedPositionSeconds = currentTrack.positionSeconds

        while (isPlaying && isEnabled && displayedPositionSeconds < track.durationSeconds) {
            delay(duration = 1_000.milliseconds)
            displayedPositionSeconds = (displayedPositionSeconds + 1).coerceAtMost(maximumValue = track.durationSeconds)
        }
    }

    LaunchedEffect(key1 = canSeek) {
        if (!canSeek) {
            seekPreviewSeconds = null
        }
    }

    val seekDescription = stringResource(resource = Res.string.currenttrack_seek_label)
    val maximumPositionSeconds = (track.durationSeconds - 1).coerceAtLeast(minimumValue = 1)
    val visiblePositionSeconds = seekPreviewSeconds?.roundToInt() ?: displayedPositionSeconds

    Column(modifier = modifier) {
        Slider(
            value = (seekPreviewSeconds ?: displayedPositionSeconds.toFloat())
                .coerceIn(minimumValue = 0f, maximumValue = maximumPositionSeconds.toFloat()),
            onValueChange = { positionSeconds -> seekPreviewSeconds = positionSeconds },
            modifier = Modifier.fillMaxWidth()
                .semantics { contentDescription = seekDescription },
            track = { sliderState ->
                SliderDefaults.Track(
                    modifier = Modifier.height(height = 4.dp),
                    sliderState = sliderState,
                    enabled = canSeek,
                    drawStopIndicator = null,
                    thumbTrackGapSize = 0.dp
                )
            },
            thumb = { _ ->
                Box(
                    modifier = Modifier.height(height = 48.dp),
                    contentAlignment = Alignment.Center
                ) {
                    Box(
                        modifier = Modifier.size(size = 10.dp)
                            .background(
                                color = if (canSeek) {
                                    MaterialTheme.colorScheme.primary
                                } else {
                                    MaterialTheme.colorScheme.onSurface.copy(alpha = 0.38f)
                                },
                                shape = CircleShape
                            )
                    )
                }
            },
            enabled = canSeek,
            valueRange = 0f..maximumPositionSeconds.toFloat(),
            onValueChangeFinished = finishSeek@{
                val previewSeconds = seekPreviewSeconds ?: return@finishSeek
                seekPreviewSeconds = null

                if (canSeek) {
                    onSeekRequested(
                        previewSeconds.roundToInt().coerceIn(minimumValue = 0, maximumValue = maximumPositionSeconds),
                        playbackOperationId
                    )
                }
            }
        )
        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.SpaceBetween
        ) {
            MText(text = visiblePositionSeconds.toDurationLabel(), fontSize = 10.sp)
            MText(text = track.durationSeconds.toDurationLabel(), fontSize = 10.sp)
        }

        if (isLoading || !canSeek) {
            MText(
                text = stringResource(
                    resource = if (isLoading) Res.string.currenttrack_seek_pending else Res.string.currenttrack_seek_unavailable
                ),
                color = MaterialTheme.colorScheme.onSurfaceVariant,
                fontSize = 10.sp
            )
        }
    }
}


@Preview
@Composable
fun PlaybackSeekBarPreview() {
    PlaybackSeekBar(
        currentTrack = CurrentTrack(
            guildId = "guildId",
            positionSeconds = 10,
            updatedAtUtc = "0L",
            currentTrack = Track(
                id = "id",
                title = "title",
                artist = "artist",
                durationSeconds = 100,
                isSeekable = true,
                playbackOperationId = "playbackOperationId"
            )
        ),
        isPlaying = true,
        isEnabled = true,
        isLoading = false,
        onSeekRequested = { _, _ -> }
    )
}
