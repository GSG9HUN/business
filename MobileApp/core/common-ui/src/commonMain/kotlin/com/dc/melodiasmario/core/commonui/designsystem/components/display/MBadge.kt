package com.dc.melodiasmario.core.commonui.designsystem.components.display

import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp

/**
 * Renders badge from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun MBadge(
    text: String,
    modifier: Modifier = Modifier,
    backgroundColor: Color,
    contentColor: Color = MaterialTheme.colorScheme.onSurface
): Unit {
    Surface(
        modifier = modifier,
        shape = RoundedCornerShape(size = 999.dp),
        color = backgroundColor
    ) {
        MText(
            modifier = Modifier.padding(horizontal = 10.dp, vertical = 4.dp),
            text = text,
            color = contentColor,
            fontWeight = FontWeight.Bold
        )
    }
}
