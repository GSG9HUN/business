package com.dc.melodiasmario.feature.currenttrack.ui.components

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.components.MArtwork
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeTokens
import com.dc.melodiasmario.feature.currentmusic.generated.resources.Res
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_empty_message
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_empty_title
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_music_icon_content_description
import org.jetbrains.compose.resources.stringResource

@Composable
fun CurrentTrackPlaceholder(
    title: String,
    contentText: String,
) {
    val colors = MelodiasMarioThemeTokens.current
    val musicIconContentDescription =
        stringResource(Res.string.currenttrack_music_icon_content_description)

    Surface(
        modifier = Modifier.fillMaxWidth().padding(12.dp),
        shape = RoundedCornerShape(12.dp),
        color = colors.surface,
        border = BorderStroke(1.dp, colors.outline),
    ) {
        Row(
            modifier = Modifier.fillMaxWidth().padding(12.dp),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.spacedBy(12.dp),
        ) {
            MArtwork(
                modifier = Modifier.size(48.dp),
                iconSize = 30.dp,
                contentDescription = musicIconContentDescription
            )
            Column(modifier = Modifier.weight(1f)) {
                MText(
                    modifier = Modifier.fillMaxWidth(),
                    text = title,
                    color = colors.textPrimary,
                    textAlign = TextAlign.Start,
                    fontWeight = FontWeight.Bold,
                )
                MText(
                    modifier = Modifier.fillMaxWidth(),
                    text = contentText,
                    color = colors.textMuted,
                    textAlign = TextAlign.Start,
                )
            }
        }
    }
}

@Preview
@Composable
fun CurrentTrackPlaceholderPreview() {
    CurrentTrackPlaceholder(
        title = stringResource(Res.string.currenttrack_empty_title),
        contentText = stringResource(Res.string.currenttrack_empty_message),
    )
}
