package com.dc.melodiasmario.core.commonui.music.components

import androidx.compose.runtime.Composable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.sp
import androidx.compose.foundation.BorderStroke
import androidx.compose.ui.Alignment
import com.dc.melodiasmario.core.commonui.components.MArtwork
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeTokens
import com.dc.melodiasmario.core.commonui.music.model.MAddMusicLabels
import com.dc.melodiasmario.core.commonui.music.model.MMusicSearchResultUi
import com.dc.melodiasmario.core.commonui.music.model.MMusicSearchKind

@Composable
fun MSelectedResultCard(
    result: MMusicSearchResultUi,
    labels: MAddMusicLabels,
    commandInFlight: Boolean,
    onAddClicked: () -> Unit,
) {
    val colors = MelodiasMarioThemeTokens.current
    Surface(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(8.dp),
        color = colors.surface,
        border = BorderStroke(1.dp, colors.primary),
    ) {
        Row(
            modifier = Modifier.fillMaxWidth().padding(10.dp),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.spacedBy(10.dp),
        ) {
            MArtwork(
                modifier = Modifier.size(44.dp),
                iconSize = 22.dp,
                imageUrl = result.thumbnailUrl,
            )
            Column(modifier = Modifier.weight(1f)) {
                MText(
                    modifier = Modifier.fillMaxWidth(),
                    text = result.title,
                    color = colors.textPrimary,
                    textAlign = TextAlign.Start,
                    fontWeight = FontWeight.Bold,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis,
                )
                MText(
                    modifier = Modifier.fillMaxWidth(),
                    text = result.subtitle(labels),
                    color = colors.textMuted,
                    textAlign = TextAlign.Start,
                    fontSize = 12.sp,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis,
                )
            }
            Button(
                onClick = onAddClicked,
                enabled = result.canEnqueue && result.canonicalUrl != null && !commandInFlight,
            ) {
                MText(
                    text = if (result.kind == MMusicSearchKind.Playlist) {
                        labels.addPlaylist
                    } else {
                        labels.addTrack
                    }
                )
            }
        }
    }
}