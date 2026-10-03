package com.dc.melodiasmario.feature.currenttrack.ui.components

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.gestures.detectDragGestures
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.runtime.mutableFloatStateOf
import androidx.compose.runtime.remember
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.input.pointer.pointerInput
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.dc.melodiasmario.core.commonui.components.MArtwork
import com.dc.melodiasmario.core.commonui.designsystem.components.button.MQueueDragHandle
import com.dc.melodiasmario.core.commonui.designsystem.components.button.MQueueRemoveButton
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeTokens
import com.dc.melodiasmario.core.commonui.formatter.toDurationLabel
import com.dc.melodiasmario.core.model.currenttrack.Track

@Composable
fun QueueTrackCard(
    queueNumber: Int,
    track: Track,
    removeContentDescription: String,
    dragHandleContentDescription: String,
    isFirst: Boolean = false,
    isLast: Boolean = false,
    onRemove: () -> Unit = {},
    onMoveUp: () -> Unit = {},
    onMoveDown: () -> Unit = {},
) {
    val colors = MelodiasMarioThemeTokens.current
    val dragOffset = remember { mutableFloatStateOf(0f) }
    val durationText = track.durationSeconds.toDurationLabel()
    val metaText = if (track.artist.isBlank()) {
        durationText
    } else {
        "${track.artist} - $durationText"
    }

    Surface(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(12.dp),
        color = colors.surface,
        border = BorderStroke(1.dp, colors.outline),
    ) {
        Row(
            modifier = Modifier.fillMaxWidth().padding(10.dp),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.spacedBy(12.dp),
        ) {
            MText(
                modifier = Modifier.width(28.dp),
                text = queueNumber.toString(),
                color = colors.textMuted,
                textAlign = TextAlign.Center,
                fontSize = 13.sp,
                fontWeight = FontWeight.Bold,
            )

            MArtwork(
                modifier = Modifier.size(48.dp),
                iconSize = 24.dp,
                imageUrl = track.thumbnailUrl,
            )

            Column(
                modifier = Modifier.weight(1f),
            ) {
                MText(
                    modifier = Modifier.fillMaxWidth(),
                    text = track.title,
                    color = colors.textPrimary,
                    textAlign = TextAlign.Start,
                    fontWeight = FontWeight.Bold,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis,
                )
                MText(
                    modifier = Modifier.fillMaxWidth(),
                    text = metaText,
                    color = colors.textMuted,
                    textAlign = TextAlign.Start,
                    fontSize = 12.sp,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis,
                )
            }

            Row(
                horizontalArrangement = Arrangement.spacedBy(6.dp),
                verticalAlignment = Alignment.CenterVertically,
            ) {
                MQueueRemoveButton(
                    contentDescription = removeContentDescription,
                    onClick = onRemove,
                )
                MQueueDragHandle(
                    modifier = Modifier.pointerInput(isFirst, isLast) {
                        detectDragGestures(
                            onDragStart = {
                                dragOffset.floatValue = 0f
                            },
                            onDragEnd = {
                                when {
                                    dragOffset.floatValue <= -QueueDragMoveThresholdPx && !isFirst -> onMoveUp()
                                    dragOffset.floatValue >= QueueDragMoveThresholdPx && !isLast -> onMoveDown()
                                }
                                dragOffset.floatValue = 0f
                            },
                            onDragCancel = {
                                dragOffset.floatValue = 0f
                            },
                            onDrag = { change, dragAmount ->
                                change.consume()
                                dragOffset.floatValue += dragAmount.y
                            },
                        )
                    },
                    contentDescription = dragHandleContentDescription,
                )
            }
        }
    }
}

private const val QueueDragMoveThresholdPx = 32f

@Preview
@Composable
fun QueueTrackCardPreview() {
    QueueTrackCard(
        queueNumber = 1,
        track = Track(
            id = "1",
            title = "Track 1",
            artist = "Artist 1",
            durationSeconds = 120,
            thumbnailUrl = "https://via.placeholder.com/150",
        ),
        removeContentDescription = "Remove",
        dragHandleContentDescription = "Drag to reorder",
    )
}
