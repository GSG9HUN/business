package com.dc.melodiasmario.feature.currenttrack.ui.components

import androidx.compose.runtime.Composable
import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Surface
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.semantics.contentDescription
import androidx.compose.ui.semantics.semantics
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.Dp
import androidx.compose.ui.unit.IntSize
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeTokens

@Composable
fun QueueActionButton(
    text: String,
    contentDescription: String,
    enabled: Boolean = true,
    onClick: () -> Unit,
) {
    val colors = MelodiasMarioThemeTokens.current

    Surface(
        modifier = Modifier
            .size(28.dp)
            .semantics { this.contentDescription = contentDescription },
        shape = RoundedCornerShape(8.dp),
        color = colors.elevated,
        border = BorderStroke(1.dp, colors.outline),
        enabled = enabled,
        onClick =onClick,
    ) {
        Box(contentAlignment = Alignment.Center) {
            MText(
                text = text,
                color = if (enabled) colors.textSecondary else colors.textMuted,
                fontSize = 13.sp,
            )
        }
    }
}

@Preview
@Composable
fun QueueActionButtonPreview() {
    QueueActionButton(
        text = "Play",
        contentDescription = "Play",
        enabled = true,
        onClick = {},
    )
}
