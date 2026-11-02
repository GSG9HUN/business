package com.dc.melodiasmario.core.commonui.designsystem.components.settings

import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.painter.Painter
import androidx.compose.ui.unit.dp

/**
 * Renders settings icon box from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun MSettingsIconBox(
    painter: Painter,
    modifier: Modifier = Modifier
): Unit {
    Surface(
        modifier = modifier.size(size = 48.dp),
        shape = RoundedCornerShape(size = 14.dp),
        color = MaterialTheme.colorScheme.surfaceContainerHighest
    ) {
        Box(
            modifier = Modifier.fillMaxSize(),
            contentAlignment = Alignment.Center
        ) {
            Icon(
                painter = painter,
                contentDescription = null,
                modifier = Modifier.size(size = 24.dp),
                tint = MaterialTheme.colorScheme.onSurface
            )
        }
    }
}
