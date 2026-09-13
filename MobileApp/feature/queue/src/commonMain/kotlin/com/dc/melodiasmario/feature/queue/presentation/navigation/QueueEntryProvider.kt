package com.dc.melodiasmario.feature.queue.presentation.navigation

import androidx.navigation3.runtime.EntryProviderScope
import com.dc.melodiasmario.core.common.navigation.AppRoute
import com.dc.melodiasmario.feature.queue.ui.QueueRoute

fun EntryProviderScope<AppRoute>.queueEntry() {
    entry<AppRoute.Queue> { route ->
        QueueRoute(guildId = route.guildId)
    }
}
