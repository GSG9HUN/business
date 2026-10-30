package com.dc.melodiasmario.core.commonui.components

import androidx.compose.runtime.Composable
import androidx.compose.runtime.remember
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.Dp
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.Res
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.playlist_cover_chill
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.playlist_cover_night_drive
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.playlist_cover_workout
import com.dc.melodiasmario.core.model.playlist.PlaylistArtworkTheme

/** Prefers API artwork and uses a bundled themed cover when the URL is absent or fails. */
@Composable
fun MPlaylistArtwork(
    playlistName: String,
    imageUrl: String?,
    modifier: Modifier = Modifier,
    iconSize: Dp = 54.dp
): Unit {
    val theme = remember(key1 = playlistName) { PlaylistArtworkTheme.fromName(name = playlistName) }
    val fallback = when (theme) {
        PlaylistArtworkTheme.NightDrive -> Res.drawable.playlist_cover_night_drive
        PlaylistArtworkTheme.Chill -> Res.drawable.playlist_cover_chill
        PlaylistArtworkTheme.Workout -> Res.drawable.playlist_cover_workout
        PlaylistArtworkTheme.Default -> null
    }
    MArtwork(
        modifier = modifier,
        imageUrl = imageUrl,
        iconSize = iconSize,
        fallbackImage = fallback
    )
}
