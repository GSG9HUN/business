package com.dc.melodiasmario.core.commonui.components

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.gestures.detectDragGesturesAfterLongPress
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
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.alpha
import androidx.compose.ui.graphics.graphicsLayer
import androidx.compose.ui.input.pointer.pointerInput
import androidx.compose.ui.layout.onGloballyPositioned
import androidx.compose.ui.semantics.CustomAccessibilityAction
import androidx.compose.ui.semantics.customActions
import androidx.compose.ui.semantics.semantics
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.compose.ui.zIndex
import com.dc.melodiasmario.core.commonui.designsystem.components.button.MQueueDragHandle
import com.dc.melodiasmario.core.commonui.designsystem.components.button.MQueueRemoveButton
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeTokens
import com.dc.melodiasmario.core.commonui.formatter.toDurationLabel
import com.dc.melodiasmario.core.commonui.reorder.ReorderableListDefaults

@Composable
fun MQueueTrackCard(
    modifier: Modifier = Modifier,
    queueNumber: Int,
    title: String,
    artist: String,
    durationSeconds: Int,
    thumbnailUrl: String? = null,
    removeContentDescription: String,
    dragHandleContentDescription: String,
    moveUpContentDescription: String,
    moveDownContentDescription: String,
    isFirst: Boolean = false,
    isLast: Boolean = false,
    onRemove: () -> Unit = {},
    onMoveUp: () -> Unit = {},
    onMoveDown: () -> Unit = {},
    onMoveDrop: (dragOffsetPx: Float, itemHeightPx: Float) -> Unit = { _, _ -> },
) {
    val colors = MelodiasMarioThemeTokens.current
    val dragOffset = remember { mutableFloatStateOf(0f) }
    val isDragging = remember { mutableStateOf(false) }
    val itemHeightPx = remember {
        mutableFloatStateOf(ReorderableListDefaults.EstimatedItemHeightPx)
    }
    val durationText = durationSeconds.toDurationLabel()
    val metaText = if (artist.isBlank()) {
        durationText
    } else {
        "$artist - $durationText"
    }
    val reorderAccessibilityActions = buildList {
        if (!isFirst) {
            add(
                CustomAccessibilityAction(
                    label = moveUpContentDescription,
                    action = {
                        onMoveUp()
                        true
                    },
                )
            )
        }

        if (!isLast) {
            add(
                CustomAccessibilityAction(
                    label = moveDownContentDescription,
                    action = {
                        onMoveDown()
                        true
                    },
                )
            )
        }
    }

    Surface(
        modifier = modifier
            .fillMaxWidth()
            .semantics {
                customActions = reorderAccessibilityActions
            }
            .zIndex(if (isDragging.value) 1f else 0f)
            .graphicsLayer {
                translationY = dragOffset.floatValue
                shadowElevation = if (isDragging.value) 16.dp.toPx() else 0f
                scaleX = if (isDragging.value) 1.02f else 1f
                scaleY = if (isDragging.value) 1.02f else 1f
            }
            .alpha(if (isDragging.value) 0.72f else 1f)
            .onGloballyPositioned { coordinates ->
                itemHeightPx.floatValue = coordinates.size.height.toFloat()
                    .coerceAtLeast(ReorderableListDefaults.EstimatedItemHeightPx)
            }
            .pointerInput(isFirst, isLast) {
                detectDragGesturesAfterLongPress(
                    onDragStart = {
                        dragOffset.floatValue = 0f
                        isDragging.value = true
                    },
                    onDragEnd = {
                        onMoveDrop(dragOffset.floatValue, itemHeightPx.floatValue)
                        dragOffset.floatValue = 0f
                        isDragging.value = false
                    },
                    onDragCancel = {
                        dragOffset.floatValue = 0f
                        isDragging.value = false
                    },
                    onDrag = { change, dragAmount ->
                        change.consume()
                        dragOffset.floatValue += dragAmount.y
                    },
                )
            },
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
                imageUrl = thumbnailUrl,
            )

            Column(
                modifier = Modifier.weight(1f),
            ) {
                MText(
                    modifier = Modifier.fillMaxWidth(),
                    text = title,
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
                    modifier = Modifier.zIndex(2f),
                    contentDescription = removeContentDescription,
                    onClick = onRemove,
                )
                MQueueDragHandle(
                    contentDescription = dragHandleContentDescription,
                )
            }
        }
    }
}

@Preview
@Composable
fun MQueueTrackCardPreview() {
    MQueueTrackCard(
        queueNumber = 1,
        title = "Track 1",
        artist = "Artist 1",
        durationSeconds = 120,
        thumbnailUrl = "https://via.placeholder.com/150",
        removeContentDescription = "Remove",
        dragHandleContentDescription = "Drag to reorder",
        moveUpContentDescription = "Move up",
        moveDownContentDescription = "Move down",
    )
}
