package com.dc.melodiasmario.feature.queue.ui.components

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.feature.queue.generated.resources.Res
import com.dc.melodiasmario.feature.queue.generated.resources.queue_edit_mode_label
import com.dc.melodiasmario.feature.queue.generated.resources.queue_edit_mode_message
import org.jetbrains.compose.resources.stringResource

/**
 * Renders queue edit mode card from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun QueueEditModeCard(
    modifier: Modifier = Modifier,
    title: String,
    message: String
): Unit {
    Surface(
        modifier = modifier,
        shape = RoundedCornerShape(size = 16.dp),
        color = MaterialTheme.colorScheme.surfaceContainerHigh,
        border = BorderStroke(width = 1.dp, color = MaterialTheme.colorScheme.outline)
    ) {
        Column(
            modifier = Modifier.padding(horizontal = 18.dp, vertical = 16.dp),
            verticalArrangement = Arrangement.spacedBy(space = 8.dp)
        ) {
            MText(
                modifier = Modifier.fillMaxWidth(),
                text = title,
                color = MaterialTheme.colorScheme.onSurfaceVariant,
                textAlign = TextAlign.Start,
                fontSize = 14.sp,
                fontWeight = FontWeight.Bold
            )
            MText(
                modifier = Modifier.fillMaxWidth(),
                text = message,
                textAlign = TextAlign.Start,
                color = MaterialTheme.colorScheme.onSurfaceVariant,
                fontSize = 13.sp
            )
        }
    }
}

@Preview
@Composable
private fun QueueEditModeCardPreview() {
    QueueEditModeCard(
        title = stringResource(resource = Res.string.queue_edit_mode_label),
        message = stringResource(resource = Res.string.queue_edit_mode_message)
    )
}
