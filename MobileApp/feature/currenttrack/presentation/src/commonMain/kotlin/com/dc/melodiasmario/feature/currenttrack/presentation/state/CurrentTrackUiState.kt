package com.dc.melodiasmario.feature.currenttrack.presentation.state

import com.dc.melodiasmario.core.commonui.music.state.MAddMusicSheetState
import com.dc.melodiasmario.feature.currenttrack.presentation.dialog.CurrentTrackDialog

data class CurrentTrackUiState(
    val isLoading: Boolean = false,
    val errorMessage: String? = null,
    val header: CurrentTrackHeaderUiState = CurrentTrackHeaderUiState(),
    val playback: CurrentTrackPlaybackUiState = CurrentTrackPlaybackUiState(),
    val queue: CurrentTrackQueueUiState = CurrentTrackQueueUiState(),
    val actions: CurrentTrackActionsUiState = CurrentTrackActionsUiState(),
    val addMusic: MAddMusicSheetState = MAddMusicSheetState(),
    val dialog: CurrentTrackDialog? = null,
)





