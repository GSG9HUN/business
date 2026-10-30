package com.dc.melodiasmario.core.commonui.designsystem.components.button

import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.DropdownMenu
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText

/**
 * Renders help tooltip button from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun MHelpTooltipButton(text: String): Unit {
    var expanded by remember { mutableStateOf(value = false) }

    Box {
        IconButton(onClick = { expanded = true }) {
            MText(
                text = "?",
                color = MaterialTheme.colorScheme.onSurfaceVariant,
                fontWeight = FontWeight.Bold
            )
        }
        DropdownMenu(
            expanded = expanded,
            onDismissRequest = { expanded = false }
        ) {
            MText(
                modifier = Modifier.padding(horizontal = 14.dp, vertical = 10.dp),
                text = text,
                color = MaterialTheme.colorScheme.onSurface,
                textAlign = TextAlign.Start
            )
        }
    }
}
