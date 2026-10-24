package com.dc.melodiasmario.feature.queue.presentation

sealed interface QueueDialog {
    data object None : QueueDialog
    data object AddMusic : QueueDialog
    data object MoreActions : QueueDialog
    data object CreatePlaylist : QueueDialog
}
