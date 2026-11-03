package com.dc.melodiasmario.core.commonui.designsystem.components.button

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.semantics.contentDescription
import androidx.compose.ui.semantics.semantics
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText

/**
 * Renders queue remove button from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun MQueueRemoveButton(
    contentDescription: String,
    onClick: () -> Unit,
    modifier: Modifier = Modifier,
    enabled: Boolean = true
): Unit {
    Surface(
        modifier = modifier
            .size(size = 36.dp)
            .semantics { this.contentDescription = contentDescription },
        shape = RoundedCornerShape(size = 9.dp),
        color = MaterialTheme.colorScheme.errorContainer,
        border = BorderStroke(width = 1.dp, color = MaterialTheme.colorScheme.error.copy(alpha = 0.28f)),
        onClick = onClick,
        enabled = enabled
    ) {
        Box(contentAlignment = Alignment.Center) {
            MText(
                text = "X",
                color = MaterialTheme.colorScheme.error,
                fontSize = 15.sp,
                fontWeight = FontWeight.Bold
            )
        }
    }
}
