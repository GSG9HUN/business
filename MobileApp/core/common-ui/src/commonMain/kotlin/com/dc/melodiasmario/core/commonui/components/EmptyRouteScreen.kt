package com.dc.melodiasmario.core.commonui.components

import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.tooling.preview.Preview
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.commonui.designsystem.theme.MmBackgroundPreviewColor

/**
 * Renders empty route screen from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun EmptyRouteScreen(
    text: String,
    modifier: Modifier = Modifier
): Unit {
    Surface(
        modifier = modifier.fillMaxSize(),
        color = MaterialTheme.colorScheme.background
    ) {
        Box(
            modifier = Modifier.fillMaxSize(),
            contentAlignment = Alignment.Center
        ) {
            MText(text = text)
        }
    }
}

@Preview(showBackground = true, backgroundColor = MmBackgroundPreviewColor)
@Composable
private fun EmptyRouteScreenPreview() {
    MelodiasMarioTheme {
        EmptyRouteScreen(text = "Coming soon")
    }
}
