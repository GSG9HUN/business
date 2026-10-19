package com.dc.melodiasmario.feature.queue.ui.components

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeTokens

@Composable
fun QueueEmptyContent(
    modifier: Modifier = Modifier,
    title: String,
    message: String,
) {
    Column(
        modifier = modifier.padding(24.dp),
        verticalArrangement = Arrangement.Center,
    ) {
        MText(
            modifier = Modifier.fillMaxWidth(),
            text = title,
            color = MelodiasMarioThemeTokens.current.textPrimary,
            textAlign = TextAlign.Center,
            fontSize = 20.sp,
            fontWeight = FontWeight.Bold,
        )
        MText(
            modifier = Modifier.fillMaxWidth().padding(top = 8.dp),
            text = message,
            color = MelodiasMarioThemeTokens.current.textMuted,
            textAlign = TextAlign.Center,
        )
    }
}