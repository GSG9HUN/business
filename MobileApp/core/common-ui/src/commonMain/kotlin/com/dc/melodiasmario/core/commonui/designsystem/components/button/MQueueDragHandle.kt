package com.dc.melodiasmario.core.commonui.designsystem.components.button

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.Canvas
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.graphics.StrokeCap
import androidx.compose.ui.semantics.contentDescription
import androidx.compose.ui.semantics.semantics
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeTokens

@Composable
fun MQueueDragHandle(
    contentDescription: String,
    modifier: Modifier = Modifier,
) {
    val colors = MelodiasMarioThemeTokens.current

    Surface(
        modifier = modifier
            .size(36.dp)
            .semantics { this.contentDescription = contentDescription },
        shape = RoundedCornerShape(9.dp),
        color = colors.elevated,
        border = BorderStroke(1.dp, colors.outline),
    ) {
        Canvas(modifier = Modifier.size(36.dp).padding(horizontal = 9.dp, vertical = 10.dp)) {
            val strokeWidth = 2.dp.toPx()
            val lineColor = colors.textMuted
            listOf(2f, size.height / 2f, size.height - 2f).forEach { y ->
                drawLine(
                    color = lineColor,
                    start = Offset(0f, y),
                    end = Offset(size.width, y),
                    strokeWidth = strokeWidth,
                    cap = StrokeCap.Round,
                )
            }
        }
    }
}
