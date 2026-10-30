package com.dc.melodiasmario.feature.playlist.presentation.navigation

import androidx.navigation3.runtime.EntryProviderScope
import com.dc.melodiasmario.core.common.navigation.AppRoute
import com.dc.melodiasmario.core.commonui.feedback.state.MToastHostState
import com.dc.melodiasmario.feature.playlist.ui.PlaylistsRoute

/**
 * Registers the playlists destination and its navigation callbacks.
 */
fun EntryProviderScope<AppRoute>.playlistsEntry(
    onPlaylistClicked: (route: AppRoute.Playlists, playlistId: String) -> Unit,
    toastHostState: MToastHostState
): Unit {
    entry<AppRoute.Playlists> { route ->
        PlaylistsRoute(
            guildId = route.guildId,
            toastHostState = toastHostState,
            onPlaylistClicked = { playlistId ->
                onPlaylistClicked(route, playlistId)
            }
        )
    }
}
