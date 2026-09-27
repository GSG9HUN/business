package com.dc.melodiasmario.core.commonui.reorder

fun calculateReorderDropTargetIndex(
    fromIndex: Int,
    itemCount: Int,
    dragOffsetPx: Float,
    itemHeightPx: Float,
    itemSpacingPx: Float,
): Int {
    if (itemCount <= 1) return fromIndex

    val itemStepPx = itemHeightPx + itemSpacingPx
    val draggedCenterY = fromIndex * itemStepPx + itemHeightPx / 2f + dragOffsetPx
    val lastIndex = itemCount - 1
    val hoveredIndex = when {
        draggedCenterY <= 0f -> 0
        draggedCenterY >= lastIndex * itemStepPx + itemHeightPx -> lastIndex
        else -> (draggedCenterY / itemStepPx).toInt().coerceIn(0, lastIndex)
    }
    val hoveredCenterY = hoveredIndex * itemStepPx + itemHeightPx / 2f
    val insertionIndex = if (draggedCenterY < hoveredCenterY) {
        hoveredIndex
    } else {
        hoveredIndex + 1
    }

    return if (insertionIndex > fromIndex) {
        insertionIndex - 1
    } else {
        insertionIndex
    }.coerceIn(0, lastIndex)
}
