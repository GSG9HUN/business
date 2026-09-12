package com.dc.melodiasmario.feature.queue.presentation.navigation

import androidx.compose.runtime.Composable
import com.dc.melodiasmario.feature.queue.ui.QueueRoute

class QueueEntryProvider {
    @Composable
    fun Entry(
        guildId: String,
    ) {
        QueueRoute(guildId = guildId)
    }
}
