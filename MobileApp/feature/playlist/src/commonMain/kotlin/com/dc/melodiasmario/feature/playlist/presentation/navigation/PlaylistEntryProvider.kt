package com.dc.melodiasmario.feature.playlist.presentation.navigation

import androidx.compose.runtime.Composable
import com.dc.melodiasmario.feature.playlist.ui.PlaylistsRoute
import com.dc.melodiasmario.feature.playlist.ui.playlistsong.PlaylistSongsRoute

class PlaylistEntryProvider {
    @Composable
    fun PlaylistsEntry(
        guildId: String,
        onPlaylistClicked: (playlistId: String) -> Unit,
    ) {
        PlaylistsRoute(
            guildId = guildId,
            onPlaylistClicked = onPlaylistClicked,
        )
    }

    @Composable
    fun PlaylistSongsEntry(
        guildId: String,
        playlistId: String,
    ) {
        PlaylistSongsRoute(
            guildId = guildId,
            playlistId = playlistId,
        )
    }
}
