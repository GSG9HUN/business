package com.dc.melodiasmario.core.commonui.reorder

data class DraggedItem(
    val fromIndex: Int,
    val headerItemCount: Int,
    val heightPx: Float,
    val grabOffsetY: Float,
    val pointerY: Float,
)