package com.dc.melodiasmario.feature.currenttrack.ui.components

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeTokens
import com.dc.melodiasmario.feature.currentmusic.generated.resources.Res
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_queue_empty
import org.jetbrains.compose.resources.stringResource

@Composable
fun QueueEmptyCard() {
    val colors = MelodiasMarioThemeTokens.current
    val emptyQueueText = stringResource(Res.string.currenttrack_queue_empty)

    Surface(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(12.dp),
        color = colors.surface,
        border = BorderStroke(1.dp, colors.outline),
    ) {
        MText(
            modifier = Modifier.padding(14.dp).fillMaxWidth(),
            text = emptyQueueText,
            color = colors.textMuted,
            textAlign = TextAlign.Start,
        )
    }
}

@Preview
@Composable
fun QueueEmptyCardPreview() {
    QueueEmptyCard()
}
