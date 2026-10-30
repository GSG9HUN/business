package com.dc.melodiasmario.core.presentation.formatter

import kotlin.math.max

/**
 * Maps this value to duration label.
 */
fun Int.toDurationLabel(): String {
    val safeSeconds = max(a = this, b = 0)
    val hours = safeSeconds / 3600
    val minutes = (safeSeconds % 3600) / 60
    val seconds = safeSeconds % 60

    return if (hours > 0) {
        "$hours:${minutes.toString()
            .padStart(2, '0')}:${seconds.toString()
            .padStart(2, '0')}"
    } else {
        "$minutes:${seconds.toString()
            .padStart(2, '0')}"
    }
}
