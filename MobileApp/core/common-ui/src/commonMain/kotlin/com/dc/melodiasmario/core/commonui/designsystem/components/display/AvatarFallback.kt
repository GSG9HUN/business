package com.dc.melodiasmario.core.commonui.designsystem.components.display

import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight

@Composable
fun AvatarFallback(name: String, contentColor: Color) {
    Box(
        modifier = Modifier.fillMaxSize(),
        contentAlignment = Alignment.Center,
    ) {
        MText(
            text = name.firstOrNull()?.uppercaseChar()?.toString() ?: "?",
            color = contentColor,
            fontWeight = FontWeight.Bold,
        )
    }
}
