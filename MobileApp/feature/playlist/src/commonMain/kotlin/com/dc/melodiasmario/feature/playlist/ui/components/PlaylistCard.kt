package com.dc.melodiasmario.feature.playlist.ui.components

import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.aspectRatio
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.size
import androidx.compose.material3.DropdownMenu
import androidx.compose.material3.DropdownMenuItem
import androidx.compose.material3.IconButton
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.getValue
import androidx.compose.runtime.setValue
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import com.dc.melodiasmario.core.ui.components.display.MText
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.semantics.contentDescription
import androidx.compose.ui.semantics.semantics
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.ui.theme.MelodiasMarioThemeTokens
import com.dc.melodiasmario.feature.playlist.domain.model.Playlist
import com.dc.melodiasmario.feature.playlist.generated.resources.Res
import com.dc.melodiasmario.feature.playlist.generated.resources.playlist_card_menu_content_description
import com.dc.melodiasmario.feature.playlist.generated.resources.playlist_card_subtitle
import com.dc.melodiasmario.feature.playlist.generated.resources.playlist_duration_hours
import com.dc.melodiasmario.feature.playlist.generated.resources.playlist_duration_hours_minutes
import com.dc.melodiasmario.feature.playlist.generated.resources.playlist_duration_minutes
import com.dc.melodiasmario.feature.playlist.generated.resources.playlist_duration_minutes_seconds
import com.dc.melodiasmario.feature.playlist.generated.resources.playlist_duration_seconds
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_delete_action
import com.dc.melodiasmario.feature.playlist.generated.resources.playlists_rename_action
import org.jetbrains.compose.resources.stringResource

@Composable
fun PlaylistCard(
    modifier: Modifier,
    playlist: Playlist,
    onClick: () -> Unit,
    onRenameClick: () -> Unit = {},
    onDeleteClick: () -> Unit = {},
) {
    var menuExpanded by remember { mutableStateOf(false) }
    val menuContentDescription =  stringResource(Res.string.playlist_card_menu_content_description)

    Surface(
        modifier = modifier.fillMaxWidth(),
        shape = RoundedCornerShape(18.dp),
        onClick = onClick,
        color = MelodiasMarioThemeTokens.current.surface,
        border = BorderStroke(
            width = 1.dp,
            color = MelodiasMarioThemeTokens.current.outline
        )
    ) {
        Box {
            Column(
                modifier = Modifier.padding(12.dp)
            ) {
                Box(
                    modifier = Modifier
                        .fillMaxWidth()
                        .aspectRatio(1f)
                        .background(
                            color = MelodiasMarioThemeTokens.current.primary,
                            shape = RoundedCornerShape(12.dp)
                        ),
                    contentAlignment = Alignment.Center
                ) {
                    //ZENE icon
                }

                Spacer(modifier = Modifier.size(12.dp))

                MText(
                    modifier = Modifier.fillMaxWidth(),
                    text = playlist.name,
                    color = MelodiasMarioThemeTokens.current.textPrimary,
                    textAlign = TextAlign.Start,
                    fontWeight = FontWeight.Bold,
                )

                MText(
                    modifier = Modifier.fillMaxWidth(),
                    text = stringResource(
                        Res.string.playlist_card_subtitle,
                        playlist.songCount,
                        playlist.duration.toDurationString(),
                    ),
                    color = MelodiasMarioThemeTokens.current.textMuted,
                    textAlign = TextAlign.Start,
                )
            }

            Box(
                modifier = Modifier
                    .fillMaxSize()
                    .padding(6.dp),
                contentAlignment = Alignment.TopEnd,
            ) {
                IconButton(
                    modifier = Modifier.semantics {
                        contentDescription = menuContentDescription
                    },
                    onClick = { menuExpanded = true }
                ) {
                    MText(
                        text = "...",
                        color = MelodiasMarioThemeTokens.current.textPrimary,
                        fontWeight = FontWeight.Bold,
                    )
                }

                DropdownMenu(
                    expanded = menuExpanded,
                    onDismissRequest = { menuExpanded = false },
                ) {
                    DropdownMenuItem(
                        text = {
                            MText(
                                text = stringResource(Res.string.playlists_rename_action),
                                textAlign = TextAlign.Start,
                            )
                        },
                        onClick = {
                            menuExpanded = false
                            onRenameClick()
                        },
                    )
                    DropdownMenuItem(
                        text = {
                            MText(
                                text = stringResource(Res.string.playlists_delete_action),
                                color = MelodiasMarioThemeTokens.current.dangerText,
                                textAlign = TextAlign.Start,
                            )
                        },
                        onClick = {
                            menuExpanded = false
                            onDeleteClick()
                        },
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
        if (minutes == 0) return stringResource(Res.string.playlist_duration_hours, hours)
        stringResource(Res.string.playlist_duration_hours_minutes, hours, minutes)
    } else if (minutes > 0) {
        if (seconds == 0) return stringResource(Res.string.playlist_duration_minutes, minutes)
        stringResource(Res.string.playlist_duration_minutes_seconds, minutes, seconds)
    } else {
        stringResource(Res.string.playlist_duration_seconds, seconds)
    }
}
