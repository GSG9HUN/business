package com.dc.melodiasmario.feature.playlisttrack.presentation.navigation

import androidx.navigation3.runtime.EntryProviderScope
import com.dc.melodiasmario.core.common.navigation.AppRoute
import com.dc.melodiasmario.core.commonui.feedback.state.MToastHostState
import com.dc.melodiasmario.feature.playlisttrack.ui.PlaylistTrackRoute

/**
 * Registers the playlist track destination and its navigation callbacks.
 */
fun EntryProviderScope<AppRoute>.playlistTrackEntry(
    onBack: () -> Unit,
    toastHostState: MToastHostState
): Unit {
    entry<AppRoute.PlaylistTrack> { route ->
        PlaylistTrackRoute(
            guildId = route.guildId,
            playlistId = route.playlistId,
            onBack = onBack,
            toastHostState = toastHostState
        )
    }
}
