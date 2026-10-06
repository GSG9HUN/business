package com.dc.melodiasmario.core.commonui.designsystem.components.button

import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeTokens
import androidx.compose.material3.Button
import androidx.compose.material3.OutlinedButton

@Composable
fun MModeButton(
    modifier: Modifier,
    text: String,
    selected: Boolean,
    onClick: () -> Unit,
) {
    if (selected) {
        Button(modifier = modifier, onClick = onClick) {
            MText(text = text, color = MelodiasMarioThemeTokens.current.textPrimary)
        }
    } else {
        OutlinedButton(modifier = modifier, onClick = onClick) {
            MText(text = text, color = MelodiasMarioThemeTokens.current.textSecondary)
        }
    }
}