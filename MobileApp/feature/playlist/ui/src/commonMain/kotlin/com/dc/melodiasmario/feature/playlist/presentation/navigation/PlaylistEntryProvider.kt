package com.dc.melodiasmario.feature.playlist.presentation.navigation

import androidx.navigation3.runtime.EntryProviderScope
import com.dc.melodiasmario.core.common.navigation.AppRoute
import com.dc.melodiasmario.core.commonui.feedback.state.MToastHostState
import com.dc.melodiasmario.feature.playlist.ui.PlaylistsRoute
import com.dc.melodiasmario.feature.playlist.ui.playlistsong.PlaylistSongsRoute

fun EntryProviderScope<AppRoute>.playlistsEntry(
    onPlaylistClicked: (route: AppRoute.Playlists, playlistId: String) -> Unit,
    toastHostState: MToastHostState,
) {
    entry<AppRoute.Playlists> { route ->
        PlaylistsRoute(
            guildId = route.guildId,
            toastHostState = toastHostState,
            onPlaylistClicked = { playlistId ->
                onPlaylistClicked(route, playlistId)
            },
        )
    }
}

fun EntryProviderScope<AppRoute>.playlistSongsEntry() {
    entry<AppRoute.PlaylistSongs> { route ->
        PlaylistSongsRoute(
            guildId = route.guildId,
            playlistId = route.playlistId,
        )
    }
}
