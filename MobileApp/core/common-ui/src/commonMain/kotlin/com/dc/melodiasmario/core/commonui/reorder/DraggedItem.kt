package com.dc.melodiasmario.core.commonui.reorder

/**
 * Provides dragged item for the reorder layer.
 *
 * @property fromIndex From index.
 * @property headerItemCount Header item count.
 * @property heightPx Height px.
 * @property grabOffsetY Grab offset y.
 * @property pointerY Pointer y.
 */
data class DraggedItem(
    val fromIndex: Int,
    val headerItemCount: Int,
    val heightPx: Float,
    val grabOffsetY: Float,
    val pointerY: Float
)
