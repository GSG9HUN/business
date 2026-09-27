package com.dc.melodiasmario.feature.currenttrack.presentation

interface CurrentTrackDialog {
    data object None : CurrentTrackDialog
    data object AddToQueue : CurrentTrackDialog
    data object MoreActions : CurrentTrackDialog
}
