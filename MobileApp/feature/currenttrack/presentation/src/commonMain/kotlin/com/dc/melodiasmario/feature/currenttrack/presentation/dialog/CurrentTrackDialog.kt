package com.dc.melodiasmario.feature.currenttrack.presentation.dialog

/**
 * Provides current track dialog for the dialog layer.
 */
interface CurrentTrackDialog {
    /**
     * Describes the none variant of [CurrentTrackDialog].
     */
    data object None : CurrentTrackDialog

    /**
     * Describes the add to queue variant of [CurrentTrackDialog].
     */
    data object AddToQueue : CurrentTrackDialog

    /**
     * Describes the more actions variant of [CurrentTrackDialog].
     */
    data object MoreActions : CurrentTrackDialog
}
