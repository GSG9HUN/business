package com.dc.melodiasmario.feature.queue.presentation

/**
 * Provides queue dialog for the presentation layer.
 */
sealed interface QueueDialog {
    /**
     * Describes the none variant of [QueueDialog].
     */
    data object None : QueueDialog

    /**
     * Describes the add music variant of [QueueDialog].
     */
    data object AddMusic : QueueDialog

    /**
     * Describes the more actions variant of [QueueDialog].
     */
    data object MoreActions : QueueDialog

    /**
     * Describes the create playlist variant of [QueueDialog].
     */
    data object CreatePlaylist : QueueDialog
}
