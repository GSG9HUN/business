package com.dc.melodiasmario.feature.audioeffect.ui.components

import androidx.compose.foundation.Canvas
import androidx.compose.foundation.ScrollState
import androidx.compose.foundation.background
import androidx.compose.foundation.gestures.Orientation
import androidx.compose.foundation.gestures.detectTapGestures
import androidx.compose.foundation.gestures.draggable
import androidx.compose.foundation.gestures.rememberDraggableState
import androidx.compose.foundation.horizontalScroll
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.rememberScrollState
import androidx.compose.material3.MaterialTheme
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.geometry.CornerRadius
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.geometry.Size
import androidx.compose.ui.input.pointer.pointerInput
import androidx.compose.ui.layout.onSizeChanged
import androidx.compose.ui.platform.LocalDensity
import androidx.compose.ui.platform.LocalLayoutDirection
import androidx.compose.ui.semantics.ProgressBarRangeInfo
import androidx.compose.ui.semantics.contentDescription
import androidx.compose.ui.semantics.progressBarRangeInfo
import androidx.compose.ui.semantics.semantics
import androidx.compose.ui.semantics.setProgress
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.LayoutDirection
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeMode
import com.dc.melodiasmario.feature.audioeffect.generated.resources.Res
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_eq_scrollbar
import kotlin.math.roundToInt
import kotlinx.coroutines.launch
import org.jetbrains.compose.resources.stringResource

/**
 * Controls the EQ row through the same scroll state as band swipes.
 * The thumb uses the measured viewport/content ratio, has a usable minimum size and supports taps,
 * dragging, accessibility adjustments and right-to-left layouts without overshooting either edge.
 */
@Composable
internal fun AudioEffectsScrollBar(scrollState: ScrollState, modifier: Modifier = Modifier) {
    val coroutineScope = rememberCoroutineScope()
    var trackWidth by remember { mutableStateOf(value = 0f) }
    val minimumThumb = with(receiver = LocalDensity.current) { 32.dp.toPx() }
    val strokeWidth = with(receiver = LocalDensity.current) { 4.dp.toPx() }
    val rightToLeft = LocalLayoutDirection.current == LayoutDirection.Rtl
    val maximum = scrollState.maxValue.takeUnless { maximumValue -> maximumValue == Int.MAX_VALUE } ?: 0
    val viewport = scrollState.viewportSize.toFloat()
    val thumbWidth = if (maximum > 0 && viewport > 0) {
        (trackWidth * viewport / (viewport + maximum)).coerceIn(
            minimumValue = minimumThumb.coerceAtMost(maximumValue = trackWidth),
            maximumValue = trackWidth
        )
    } else {
        trackWidth
    }
    val travel = (trackWidth - thumbWidth).coerceAtLeast(minimumValue = 0f)
    val progress = if (maximum > 0) scrollState.value.toFloat() / maximum else 0f
    val visualProgress = if (rightToLeft) 1f - progress else progress
    val trackColor = MaterialTheme.colorScheme.outlineVariant.copy(alpha = .5f)
    val thumbColor = MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = .65f)
    val description = stringResource(resource = Res.string.audio_effects_eq_scrollbar)
    val dragState = rememberDraggableState { delta ->

        if (travel > 0 && maximum > 0) {
            val direction = if (rightToLeft) -1f else 1f
            val target = (scrollState.value + delta * direction * maximum / travel).roundToInt()
                .coerceIn(minimumValue = 0, maximumValue = maximum)
            coroutineScope.launch { scrollState.scrollTo(value = target) }
        }
    }

    Canvas(
        modifier = modifier.fillMaxWidth()
            .height(height = 24.dp)
            .onSizeChanged { size -> trackWidth = size.width.toFloat() }
            .semantics {
                contentDescription = description
                progressBarRangeInfo = ProgressBarRangeInfo(current = progress, range = 0f..1f)
                setProgress { requestedProgress ->
                    val targetProgress = requestedProgress.coerceIn(minimumValue = 0f, maximumValue = 1f)
                    coroutineScope.launch {
                        scrollState.scrollTo(value = (targetProgress * maximum).roundToInt())
                    }
                    true
                }
            }
            .pointerInput(key1 = scrollState, key2 = listOf(maximum, trackWidth, rightToLeft, viewport)) {
                detectTapGestures { position ->

                    if (travel > 0 && maximum > 0) {
                        val fraction = ((position.x - thumbWidth / 2) / travel).coerceIn(minimumValue = 0f, maximumValue = 1f)
                        val targetProgress = if (rightToLeft) 1f - fraction else fraction
                        coroutineScope.launch { scrollState.scrollTo(value = (targetProgress * maximum).roundToInt()) }
                    }
                }
            }
            .draggable(state = dragState, orientation = Orientation.Horizontal, enabled = maximum > 0)
    ) {
        val top = (size.height - strokeWidth) / 2
        drawRoundRect(
            color = trackColor, topLeft = Offset(x = 0f, y = top),
            size = Size(width = size.width, height = strokeWidth), cornerRadius = CornerRadius(x = strokeWidth / 2)
        )
        drawRoundRect(
            color = thumbColor, topLeft = Offset(x = travel * visualProgress, y = top),
            size = Size(width = thumbWidth, height = strokeWidth), cornerRadius = CornerRadius(x = strokeWidth / 2)
        )
    }
}

@Preview(showBackground = true)
@Composable
private fun AudioEffectsScrollBarPreview() {
    MelodiasMarioTheme(themeMode = MelodiasMarioThemeMode.Dark) {
        val scrollState = rememberScrollState()
        Column(modifier = Modifier.width(width = 280.dp)) {
            Row(modifier = Modifier.horizontalScroll(state = scrollState)) {
                repeat(times = 15) {
                    Box(
                        modifier = Modifier.width(width = 48.dp)
                            .height(height = 100.dp)
                            .background(color = MaterialTheme.colorScheme.surfaceContainerHigh)
                    )
                }
            }
            AudioEffectsScrollBar(scrollState = scrollState)
        }
    }
}
