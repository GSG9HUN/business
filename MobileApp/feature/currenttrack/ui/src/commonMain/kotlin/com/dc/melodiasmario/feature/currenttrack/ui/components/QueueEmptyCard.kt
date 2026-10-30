package com.dc.melodiasmario.feature.currenttrack.ui.components

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.feature.currenttrack.generated.resources.Res
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_queue_empty
import org.jetbrains.compose.resources.stringResource

/**
 * Renders queue empty card from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun QueueEmptyCard(): Unit {
    val emptyQueueText = stringResource(resource = Res.string.currenttrack_queue_empty)

    Surface(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(size = 12.dp),
        color = MaterialTheme.colorScheme.surface,
        border = BorderStroke(width = 1.dp, color = MaterialTheme.colorScheme.outline)
    ) {
        MText(
            modifier = Modifier.padding(all = 14.dp)
                .fillMaxWidth(),
            text = emptyQueueText,
            color = MaterialTheme.colorScheme.onSurfaceVariant,
            textAlign = TextAlign.Start
        )
    }
}

@Preview
@Composable
private fun QueueEmptyCardPreview() {
    QueueEmptyCard()
}
