package com.dc.melodiasmario.feature.currenttrack.presentation.navigation

import androidx.navigation3.runtime.EntryProviderScope
import com.dc.melodiasmario.core.common.navigation.AppRoute
import com.dc.melodiasmario.core.commonui.feedback.state.MToastHostState
import com.dc.melodiasmario.feature.currenttrack.ui.CurrentTrackRoute

fun EntryProviderScope<AppRoute>.currentTrackEntry(
    onGuildClicked: () -> Unit,
    onProfileClicked: () -> Unit,
    toastHostState: MToastHostState
) {
    entry<AppRoute.CurrentTrack> { route ->
        CurrentTrackRoute(
            guildId = route.guildId,
            onGuildClicked = onGuildClicked,
            onProfileClicked = onProfileClicked,
            toastHostState = toastHostState
        )
    }
}
