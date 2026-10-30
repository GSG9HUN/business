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

/**
 * Renders reorder edge auto scroll from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun ReorderEdgeAutoScroll(
    listState: LazyListState,
    dragPositionY: Float?,
    viewportHeightPx: Float,
    edgeSize: Dp = 64.dp,
    speedDpPerSecond: Float = 500f
): Unit {
    val density = LocalDensity.current
    val edgeSizePx = with(receiver = density) { edgeSize.toPx() }
    val speedPxPerSecond = with(receiver = density) {
        speedDpPerSecond.dp.toPx()
    }

    val currentDragPositionY = rememberUpdatedState(newValue = dragPositionY)
    val isDragging = dragPositionY != null

    LaunchedEffect(
        listState,
        isDragging,
        viewportHeightPx,
        edgeSizePx,
        speedPxPerSecond
    ) {
        if (!isDragging || viewportHeightPx <= 0f) {
            return@LaunchedEffect
        }

        val edgePx = edgeSizePx.coerceIn(
            minimumValue = 0f,
            maximumValue = viewportHeightPx / 2f
        )

        var previousFrame = withFrameNanos { frameTimeNanos -> frameTimeNanos }

        while (isActive) {
            val currentFrame = withFrameNanos { frameTimeNanos -> frameTimeNanos }
            val elapsedSeconds =
                ((currentFrame - previousFrame) / 1_000_000_000f).coerceAtMost(maximumValue = 0.05f)

            previousFrame = currentFrame

            val pointerY = currentDragPositionY.value ?: break

            val direction = when {
                pointerY < edgePx -> -1f
                pointerY > viewportHeightPx - edgePx -> 1f
                else -> 0f
            }

            if (direction != 0f) {
                listState.scrollBy(
                    value = direction * speedPxPerSecond * elapsedSeconds
                )
            }
        }
    }
}
