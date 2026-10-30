package com.dc.melodiasmario.feature.currenttrack.ui.components

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.MaterialTheme
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
import com.dc.melodiasmario.feature.currenttrack.generated.resources.Res
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_empty_message
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_empty_title
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_music_icon_content_description
import org.jetbrains.compose.resources.stringResource

/**
 * Renders current track placeholder from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun CurrentTrackPlaceholder(
    title: String,
    contentText: String
): Unit {
    val musicIconContentDescription =
        stringResource(resource = Res.string.currenttrack_music_icon_content_description)

    Surface(
        modifier = Modifier.fillMaxWidth()
            .padding(all = 12.dp),
        shape = RoundedCornerShape(size = 12.dp),
        color = MaterialTheme.colorScheme.surface,
        border = BorderStroke(width = 1.dp, color = MaterialTheme.colorScheme.outline)
    ) {
        Row(
            modifier = Modifier.fillMaxWidth()
                .padding(all = 12.dp),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.spacedBy(space = 12.dp)
        ) {
            MArtwork(
                modifier = Modifier.size(size = 48.dp),
                iconSize = 30.dp,
                contentDescription = musicIconContentDescription
            )
            Column(modifier = Modifier.weight(weight = 1f)) {
                MText(
                    modifier = Modifier.fillMaxWidth(),
                    text = title,
                    color = MaterialTheme.colorScheme.onSurface,
                    textAlign = TextAlign.Start,
                    fontWeight = FontWeight.Bold
                )
                MText(
                    modifier = Modifier.fillMaxWidth(),
                    text = contentText,
                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                    textAlign = TextAlign.Start
                )
            }
        }
    }
}

@Preview
@Composable
private fun CurrentTrackPlaceholderPreview() {
    CurrentTrackPlaceholder(
        title = stringResource(resource = Res.string.currenttrack_empty_title),
        contentText = stringResource(resource = Res.string.currenttrack_empty_message)
    )
}
