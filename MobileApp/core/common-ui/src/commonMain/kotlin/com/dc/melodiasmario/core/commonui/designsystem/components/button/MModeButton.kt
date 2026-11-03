package com.dc.melodiasmario.core.commonui.designsystem.components.button

import androidx.compose.material3.Button
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText

/**
 * Renders mode button from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun MModeButton(
    modifier: Modifier,
    text: String,
    selected: Boolean,
    onClick: () -> Unit
): Unit {
    if (selected) {
        Button(modifier = modifier, onClick = onClick) {
            MText(text = text, color = MaterialTheme.colorScheme.onPrimary)
        }
    } else {
        OutlinedButton(modifier = modifier, onClick = onClick) {
            MText(text = text, color = MaterialTheme.colorScheme.onSurfaceVariant)
        }
    }
}
