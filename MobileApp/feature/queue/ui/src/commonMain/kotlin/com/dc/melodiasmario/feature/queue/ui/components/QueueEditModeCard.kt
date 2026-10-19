package com.dc.melodiasmario.feature.queue.ui.components

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.sp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeTokens

@Composable
fun QueueEditModeCard(
    modifier: Modifier = Modifier,
    title: String,
    message: String,
) {
    val colors = MelodiasMarioThemeTokens.current

    Surface(
        modifier = modifier,
        shape = RoundedCornerShape(16.dp),
        color = colors.elevated,
        border = BorderStroke(1.dp, colors.outline),
    ) {
        Column(
            modifier = Modifier.padding(horizontal = 18.dp, vertical = 16.dp),
            verticalArrangement = Arrangement.spacedBy(8.dp),
        ) {
            MText(
                modifier = Modifier.fillMaxWidth(),
                text = title,
                color = colors.textSecondary,
                textAlign = TextAlign.Start,
                fontSize = 14.sp,
                fontWeight = FontWeight.Bold,
            )
            MText(
                modifier = Modifier.fillMaxWidth(),
                text = message,
                textAlign = TextAlign.Start,
                color = colors.textMuted,
                fontSize = 13.sp,
            )
        }
    }
}

@Preview
@Composable
fun QueueEditModeCardPreview() {
    QueueEditModeCard(
        title = "Edit Mode",
        message = "You can now reorder or remove tracks from the queue, but the string should be at least 2 lines long to see how it looks when the text wraps to the next line.",
    )
}
