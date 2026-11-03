package com.dc.melodiasmario.feature.queue.presentation.navigation

import androidx.navigation3.runtime.EntryProviderScope
import com.dc.melodiasmario.core.common.navigation.AppRoute
import com.dc.melodiasmario.core.commonui.feedback.state.MToastHostState
import com.dc.melodiasmario.feature.queue.ui.QueueRoute

/**
 * Registers the queue destination and its navigation callbacks.
 */
fun EntryProviderScope<AppRoute>.queueEntry(onGuildClicked: () -> Unit, toastHostState: MToastHostState): Unit {
    entry<AppRoute.Queue> { route ->
        QueueRoute(guildId = route.guildId, onGuildClicked = onGuildClicked, toastHostState = toastHostState)
    }
}
