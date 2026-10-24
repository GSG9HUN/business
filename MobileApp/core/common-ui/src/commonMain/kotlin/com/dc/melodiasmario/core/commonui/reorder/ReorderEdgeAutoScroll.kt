package com.dc.melodiasmario.core.commonui.reorder

import androidx.compose.foundation.gestures.scrollBy
import androidx.compose.foundation.lazy.LazyListState
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.rememberUpdatedState
import androidx.compose.runtime.withFrameNanos
import androidx.compose.ui.platform.LocalDensity
import androidx.compose.ui.unit.Dp
import androidx.compose.ui.unit.dp
import kotlinx.coroutines.isActive

@Composable
fun ReorderEdgeAutoScroll(
    listState: LazyListState,
    dragPositionY: Float?,
    viewportHeightPx: Float,
    edgeSize: Dp = 64.dp,
    speedDpPerSecond: Float = 500f,
) {
    val density = LocalDensity.current
    val edgeSizePx = with(density) { edgeSize.toPx() }
    val speedPxPerSecond = with(density) {
        speedDpPerSecond.dp.toPx()
    }

    val currentDragPositionY = rememberUpdatedState(dragPositionY)
    val isDragging = dragPositionY != null

    LaunchedEffect(
        listState,
        isDragging,
        viewportHeightPx,
        edgeSizePx,
        speedPxPerSecond,
    ) {
        if (!isDragging || viewportHeightPx <= 0f) {
            return@LaunchedEffect
        }

        val edgePx = edgeSizePx.coerceIn(
            minimumValue = 0f,
            maximumValue = viewportHeightPx / 2f,
        )

        var previousFrame = withFrameNanos { it }

        while (isActive) {
            val currentFrame = withFrameNanos { it }
            val elapsedSeconds =
                ((currentFrame - previousFrame) / 1_000_000_000f).coerceAtMost(0.05f)

            previousFrame = currentFrame

            val pointerY = currentDragPositionY.value ?: break

            val direction = when {
                pointerY < edgePx -> -1f
                pointerY > viewportHeightPx - edgePx -> 1f
                else -> 0f
            }

            if (direction != 0f) {
                listState.scrollBy(
                    direction * speedPxPerSecond * elapsedSeconds
                )
            }
        }
    }
}