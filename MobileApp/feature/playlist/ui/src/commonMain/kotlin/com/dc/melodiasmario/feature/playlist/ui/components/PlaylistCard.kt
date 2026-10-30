package com.dc.melodiasmario.feature.playlist.ui.components

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.aspectRatio
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.DropdownMenu
import androidx.compose.material3.DropdownMenuItem
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.semantics.contentDescription
import androidx.compose.ui.semantics.semantics
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.components.MPlaylistArtwork
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.commonui.designsystem.theme.MmBackgroundPreviewColor
import com.dc.melodiasmario.core.model.playlist.Playlist
import com.dc.melodiasmario.feature.playlist.generated.resources.Res
import com.dc.melodiasmario.feature.playlist.generated.resources.playlist_card_menu_content_description
import com.dc.melodiasmario.feature.playlist.generated.resources.playlist_card_subtitle
import com.dc.melodiasmario.feature.playlist.generated.resources.playlist_duration_hours
import com.dc.melodiasmario.feature.playlist.generated.resources.playlist_duration_hours_minutes
import com.dc.melodiasmario.feature.playlist.generated.resources.playlist_duration_minutes
import com.dc.melodiasmario.feature.playlist.generated.resources.playlist_duration_minutes_seconds
import com.dc.melodiasmario.feature.playlist.generated.resources.playlist_duration_seconds
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_artwork_title
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_delete_action
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_rename_action
import org.jetbrains.compose.resources.stringResource

/**
 * Renders playlist card from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun PlaylistCard(
    modifier: Modifier,
    playlist: Playlist,
    onClick: () -> Unit,
    onRenameClick: () -> Unit = {},
    onDeleteClick: () -> Unit = {},
    onArtworkClick: () -> Unit = {}
): Unit {
    var menuExpanded by remember { mutableStateOf(value = false) }
    val menuContentDescription =  stringResource(resource = Res.string.playlist_card_menu_content_description)

    Surface(
        modifier = modifier.fillMaxWidth(),
        shape = RoundedCornerShape(size = 18.dp),
        onClick = onClick,
        color = MaterialTheme.colorScheme.surface,
        border = BorderStroke(
            width = 1.dp,
            color = MaterialTheme.colorScheme.outline
        )
    ) {
        Box {
            Column(
                modifier = Modifier.padding(all = 12.dp)
            ) {
                MPlaylistArtwork(
                    playlistName = playlist.name,
                    modifier = Modifier
                        .fillMaxWidth()
                        .aspectRatio(ratio = 1f)
                        .background(
                            color = MaterialTheme.colorScheme.primary,
                            shape = RoundedCornerShape(size = 12.dp)
                        ),
                    imageUrl = playlist.artworkUri
                )

                Spacer(modifier = Modifier.size(size = 12.dp))

                MText(
                    modifier = Modifier.fillMaxWidth(),
                    text = playlist.name,
                    color = MaterialTheme.colorScheme.onSurface,
                    textAlign = TextAlign.Start,
                    fontWeight = FontWeight.Bold
                )

                MText(
                    modifier = Modifier.fillMaxWidth(),
                    text = stringResource(
                        resource = Res.string.playlist_card_subtitle,
                        formatArgs = arrayOf<Any>(playlist.songCount, playlist.duration.toDurationString())
                    ),
                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                    textAlign = TextAlign.Start
                )
            }

            Box(
                modifier = Modifier
                    .fillMaxSize()
                    .padding(all = 6.dp),
                contentAlignment = Alignment.TopEnd
            ) {
                IconButton(
                    modifier = Modifier.semantics {
                        contentDescription = menuContentDescription
                    },
                    onClick = { menuExpanded = true }
                ) {
                    MText(
                        text = "...",
                        color = MaterialTheme.colorScheme.onSurface,
                        fontWeight = FontWeight.Bold
                    )
                }

                DropdownMenu(
                    expanded = menuExpanded,
                    onDismissRequest = { menuExpanded = false }
                ) {
                    DropdownMenuItem(
                        text = { MText(text = stringResource(resource = Res.string.playlists_artwork_title)) },
                        onClick = {
                            menuExpanded = false
                            onArtworkClick()
                        }
                    )
                    DropdownMenuItem(
                        text = {
                            MText(
                                text = stringResource(resource = Res.string.playlists_rename_action),
                                textAlign = TextAlign.Start
                            )
                        },
                        onClick = {
                            menuExpanded = false
                            onRenameClick()
                        }
                    )
                    DropdownMenuItem(
                        text = {
                            MText(
                                text = stringResource(resource = Res.string.playlists_delete_action),
                                color = MaterialTheme.colorScheme.error,
                                textAlign = TextAlign.Start
                            )
                        },
                        onClick = {
                            menuExpanded = false
                            onDeleteClick()
                        }
                    )
                }
            }
        }
    }
}

@Composable
private fun Int.toDurationString(): String {
    val hours = this / 3600
    val minutes = (this % 3600) / 60
    val seconds = this % 60

    return if (hours > 0) {
        if (minutes == 0) {
            return stringResource(resource = Res.string.playlist_duration_hours, formatArgs = arrayOf<Any>(hours))
        }

        stringResource(resource = Res.string.playlist_duration_hours_minutes, formatArgs = arrayOf<Any>(hours, minutes))
    } else if (minutes > 0) {
        if (seconds == 0) {
            return stringResource(resource = Res.string.playlist_duration_minutes, formatArgs = arrayOf<Any>(minutes))
        }

        stringResource(resource = Res.string.playlist_duration_minutes_seconds, formatArgs = arrayOf<Any>(minutes, seconds))
    } else {
        stringResource(resource = Res.string.playlist_duration_seconds, formatArgs = arrayOf<Any>(seconds))
    }
}

@Preview(showBackground = true, backgroundColor = MmBackgroundPreviewColor)
@Composable
private fun PlaylistCardPreview() {
    MelodiasMarioTheme {
        PlaylistCard(
            modifier = Modifier.padding(all = 16.dp),
            playlist = Playlist(
                id = "favorites",
                name = "Late night queue",
                songCount = 42,
                duration = 12_840
            ),
            onClick = {}
        )
    }
}
