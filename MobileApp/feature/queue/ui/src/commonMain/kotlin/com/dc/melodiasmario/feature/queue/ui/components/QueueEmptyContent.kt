package com.dc.melodiasmario.feature.queue.ui.components

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.MaterialTheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText

/**
 * Renders queue empty content from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun QueueEmptyContent(
    modifier: Modifier = Modifier,
    title: String,
    message: String
): Unit {
    Column(
        modifier = modifier.padding(all = 24.dp),
        verticalArrangement = Arrangement.Center
    ) {
        MText(
            modifier = Modifier.fillMaxWidth(),
            text = title,
            color = MaterialTheme.colorScheme.onSurface,
            textAlign = TextAlign.Center,
            fontSize = 20.sp,
            fontWeight = FontWeight.Bold
        )
        MText(
            modifier = Modifier.fillMaxWidth()
                .padding(top = 8.dp),
            text = message,
            color = MaterialTheme.colorScheme.onSurfaceVariant,
            textAlign = TextAlign.Center
        )
    }
}
