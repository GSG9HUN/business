package com.dc.melodiasmario.core.commonui.designsystem.components.button

import androidx.compose.runtime.Composable
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.DropdownMenu
import androidx.compose.material3.IconButton
import androidx.compose.runtime.getValue
import androidx.compose.runtime.setValue
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeTokens

@Composable
fun MHelpTooltipButton(text: String) {
    var expanded by remember { mutableStateOf(false) }
    val colors = MelodiasMarioThemeTokens.current

    Box {
        IconButton(onClick = { expanded = true }) {
            MText(
                text = "?",
                color = colors.textSecondary,
                fontWeight = FontWeight.Bold,
            )
        }
        DropdownMenu(
            expanded = expanded,
            onDismissRequest = { expanded = false },
        ) {
            MText(
                modifier = Modifier.padding(horizontal = 14.dp, vertical = 10.dp),
                text = text,
                color = colors.textPrimary,
                textAlign = TextAlign.Start,
            )
        }
    }
}