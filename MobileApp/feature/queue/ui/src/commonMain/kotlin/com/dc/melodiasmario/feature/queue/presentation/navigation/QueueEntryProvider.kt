package com.dc.melodiasmario.feature.queue.presentation.navigation

import androidx.navigation3.runtime.EntryProviderScope
import com.dc.melodiasmario.core.common.navigation.AppRoute
import com.dc.melodiasmario.feature.queue.ui.QueueRoute
import com.dc.melodiasmario.core.commonui.feedback.state.MToastHostState

fun EntryProviderScope<AppRoute>.queueEntry(onGuildClicked: () -> Unit, toastHostState: MToastHostState) {
    entry<AppRoute.Queue> { route ->
        QueueRoute(guildId = route.guildId, onGuildClicked = onGuildClicked, toastHostState = toastHostState)
    }
}
