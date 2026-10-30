package com.dc.melodiasmario.feature.playlisttrack.ui.components

import androidx.compose.foundation.layout.*
import androidx.compose.material3.Button
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.MaterialTheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.dc.melodiasmario.core.commonui.components.MPlaylistArtwork
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.feature.playlisttrack.generated.resources.*
import org.jetbrains.compose.resources.stringResource

@Composable
internal fun PlaylistTrackHeader(
    name: String,
    trackCount: Int,
    totalMinutes: Long,
    enabled: Boolean,
    isBusy: Boolean,
    onLoad: () -> Unit,
    modifier: Modifier = Modifier,
    artworkUri: String? = null,
) {
    Row(
        modifier = modifier.fillMaxWidth().padding(vertical = 16.dp),
        horizontalArrangement = Arrangement.spacedBy(16.dp),
        verticalAlignment = Alignment.CenterVertically,
    ) {
        MPlaylistArtwork(playlistName = name, modifier = Modifier.size(92.dp), iconSize = 40.dp, imageUrl = artworkUri)
        Column(modifier = Modifier.weight(1f), verticalArrangement = Arrangement.spacedBy(8.dp)) {
            MText(
                text = name,
                color = MaterialTheme.colorScheme.onSurface,
                fontSize = 22.sp,
                fontWeight = FontWeight.Bold,
                maxLines = 2,
                overflow = TextOverflow.Ellipsis,
            )
            MText(text = stringResource(Res.string.playlisttrack_summary, trackCount, totalMinutes))
            Button(
                onClick = onLoad,
                enabled = enabled,
                modifier = Modifier.fillMaxWidth(),
            ) {
                if (isBusy) {
                    CircularProgressIndicator(
                        modifier = Modifier.size(16.dp),
                        strokeWidth = 2.dp,
                        color = MaterialTheme.colorScheme.onSurfaceVariant,
                    )
                    Spacer(Modifier.width(8.dp))
                }
                MText(
                    text = stringResource(if (isBusy) Res.string.playlisttrack_processing else Res.string.playlisttrack_load),
                    color = if (enabled) MaterialTheme.colorScheme.onPrimary else MaterialTheme.colorScheme.onSurfaceVariant,
                    fontWeight = FontWeight.Bold,
                )
            }
        }
    }
}
