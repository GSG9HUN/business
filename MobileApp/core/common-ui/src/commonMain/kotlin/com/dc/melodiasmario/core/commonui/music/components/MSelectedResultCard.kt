package com.dc.melodiasmario.core.commonui.music.components

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Button
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.dc.melodiasmario.core.commonui.components.MArtwork
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.music.model.MAddMusicLabels
import com.dc.melodiasmario.core.presentation.music.model.MMusicSearchKind
import com.dc.melodiasmario.core.presentation.music.model.MMusicSearchResultUi

/**
 * Renders selected result card from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun MSelectedResultCard(
    result: MMusicSearchResultUi,
    mAddMusicLabels: MAddMusicLabels,
    commandInFlight: Boolean,
    onAddClicked: () -> Unit
): Unit {
    Surface(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(size = 8.dp),
        color = MaterialTheme.colorScheme.surface,
        border = BorderStroke(width = 1.dp, color = MaterialTheme.colorScheme.primary)
    ) {
        Row(
            modifier = Modifier.fillMaxWidth()
                .padding(all = 10.dp),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.spacedBy(space = 10.dp)
        ) {
            MArtwork(
                modifier = Modifier.size(size = 44.dp),
                iconSize = 22.dp,
                imageUrl = result.thumbnailUrl
            )
            Column(modifier = Modifier.weight(weight = 1f)) {
                MText(
                    modifier = Modifier.fillMaxWidth(),
                    text = result.title,
                    color = MaterialTheme.colorScheme.onSurface,
                    textAlign = TextAlign.Start,
                    fontWeight = FontWeight.Bold,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis
                )
                MText(
                    modifier = Modifier.fillMaxWidth(),
                    text = result.subtitle(mAddMusicLabels = mAddMusicLabels),
                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                    textAlign = TextAlign.Start,
                    fontSize = 12.sp,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis
                )
            }
            Button(
                onClick = onAddClicked,
                enabled = result.canEnqueue && result.canonicalUrl != null && !commandInFlight
            ) {
                MText(
                    text = if (result.kind == MMusicSearchKind.Playlist) {
                        mAddMusicLabels.addPlaylist
                    } else {
                        mAddMusicLabels.addTrack
                    }
                )
            }
        }
    }
}
