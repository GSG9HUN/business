package com.dc.melodiasmario.core.commonui.reorder

import androidx.compose.foundation.gestures.awaitEachGesture
import androidx.compose.foundation.gestures.awaitFirstDown
import androidx.compose.foundation.gestures.awaitLongPressOrCancellation
import androidx.compose.foundation.gestures.drag
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.offset
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyListState
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableFloatStateOf
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberUpdatedState
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clipToBounds
import androidx.compose.ui.input.pointer.changedToUp
import androidx.compose.ui.input.pointer.pointerInput
import androidx.compose.ui.layout.onSizeChanged
import androidx.compose.ui.semantics.clearAndSetSemantics
import androidx.compose.ui.unit.Dp
import androidx.compose.ui.unit.IntOffset
import androidx.compose.ui.unit.dp
import kotlin.math.roundToInt

/**
 * Renders reorder drag container from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun ReorderDragContainer(
    listState: LazyListState,
    itemKeys: List<String>,
    enabled: Boolean,
    modifier: Modifier = Modifier,
    horizontalContentPadding: Dp = 12.dp,
    onMove: (fromIndex: Int, toIndex: Int) -> Unit,
    dragOverlay: @Composable (index: Int) -> Unit,
    content: @Composable (draggedIndex: Int?) -> Unit
): Unit {
    var draggedItem by remember {
        mutableStateOf<DraggedItem?>(value = null)
    }
    var viewportHeightPx by remember {
        mutableFloatStateOf(value = 0f)
    }

    val currentOnMove by rememberUpdatedState(newValue = onMove)
    val indexByKey = remember(key1 = itemKeys) {
        itemKeys.withIndex()
            .associate { (index, key) -> key to index }
    }

    Box(
        modifier = modifier
            .clipToBounds()
            .onSizeChanged { layoutSize ->
                viewportHeightPx = layoutSize.height.toFloat()
            }
            .pointerInput(listState, itemKeys, enabled) {
                if (!enabled) {
                    return@pointerInput
                }

                awaitEachGesture {
                    val down = awaitFirstDown(requireUnconsumed = false)
                    val initialLayout = listState.layoutInfo
                    val downY = down.position.y + initialLayout.viewportStartOffset

                    val initialItem =
                        initialLayout.visibleItemsInfo.firstOrNull { item ->
                            item.key in indexByKey
                                    && downY >= item.offset
                                    && downY < item.offset + item.size
                        } ?: return@awaitEachGesture

                    val longPress = awaitLongPressOrCancellation(pointerId = down.id)
                        ?: return@awaitEachGesture

                    val layout = listState.layoutInfo
                    val item = layout.visibleItemsInfo.firstOrNull { visibleItemsInfo -> visibleItemsInfo.key == initialItem.key }
                        ?: return@awaitEachGesture

                    val fromIndex = indexByKey[item.key]
                        ?: return@awaitEachGesture

                    val pointerY = longPress.position.y
                    val itemTopY = item.offset - layout.viewportStartOffset

                    draggedItem = DraggedItem(
                        fromIndex = fromIndex,
                        headerItemCount = item.index - fromIndex,
                        heightPx = item.size.toFloat(),
                        grabOffsetY = pointerY - itemTopY,
                        pointerY = pointerY
                    )

                    var move: Pair<Int, Int>? = null

                    try {
                        val completed = drag(pointerId = longPress.id) { change ->
                            change.consume()

                            draggedItem = draggedItem?.copy(pointerY = change.position.y)
                        }

                        if (completed) {
                            currentEvent.changes.forEach { change ->

                                if (change.changedToUp()) {
                                    change.consume()
                                }
                            }

                            val currentItem = draggedItem

                            if (currentItem != null) {
                                val targetIndex = calculateTargetIndex(
                                    draggedItem = currentItem,
                                    listState = listState,
                                    indexByKey = indexByKey,
                                    itemCount = itemKeys.size
                                )

                                if (targetIndex != currentItem.fromIndex) {
                                    move = currentItem.fromIndex to targetIndex
                                }
                            }
                        }
                    } finally {
                        draggedItem = null
                    }

                    move?.let { (fromIndex, toIndex) ->
                        currentOnMove(fromIndex, toIndex)
                    }
                }
            }
    ) {
        content(draggedItem?.fromIndex)

        ReorderEdgeAutoScroll(
            listState = listState,
            dragPositionY = draggedItem?.pointerY,
            viewportHeightPx = viewportHeightPx
        )

        draggedItem?.let { item ->
            Box(
                modifier = Modifier
                    .offset {
                        IntOffset(
                            x = 0,
                            y = (item.pointerY - item.grabOffsetY).roundToInt()
                        )
                    }
                    .fillMaxWidth()
                    .padding(horizontal = horizontalContentPadding)
                    .clearAndSetSemantics {}
            ) {
                dragOverlay(item.fromIndex)
            }
        }
    }
}

private fun calculateTargetIndex(
    draggedItem: DraggedItem,
    listState: LazyListState,
    indexByKey: Map<String, Int>,
    itemCount: Int
): Int {
    val layout = listState.layoutInfo
    val draggedCenterY =
        draggedItem.pointerY -
                draggedItem.grabOffsetY +
                draggedItem.heightPx / 2f +
                layout.viewportStartOffset

    val visibleQueueItems = layout.visibleItemsInfo
        .mapNotNull { item ->
            val queueIndex = indexByKey[item.key]
                ?: return@mapNotNull null

            if (queueIndex == draggedItem.fromIndex) {
                return@mapNotNull null
            }

            queueIndex to (item.offset + item.size / 2f)
        }
        .sortedBy { indexedItem -> indexedItem.first }

    if (visibleQueueItems.isEmpty()) {
        val onlyHeadersVisible =
            layout.visibleItemsInfo.isNotEmpty() &&
                    layout.visibleItemsInfo.all { visibleItemsInfo ->
                        visibleItemsInfo.index < draggedItem.headerItemCount
                    }

        return if (onlyHeadersVisible) {
            0
        } else {
            draggedItem.fromIndex
        }
    }

    val insertionIndex = visibleQueueItems
        .firstOrNull { (_, centerY) ->
            draggedCenterY < centerY
        }
        ?.first
        ?: (visibleQueueItems.last().first + 1)

    val targetIndex = if (insertionIndex > draggedItem.fromIndex) {
        insertionIndex - 1
    } else {
        insertionIndex
    }

    return targetIndex.coerceIn(0, itemCount - 1)
}
