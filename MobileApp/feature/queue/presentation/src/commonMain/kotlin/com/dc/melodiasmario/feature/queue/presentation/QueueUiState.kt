package com.dc.melodiasmario.feature.queue.presentation

import com.dc.melodiasmario.core.commonui.music.state.MAddMusicSheetState
import com.dc.melodiasmario.core.model.currenttrack.Track

data class QueueUiState(
    val isLoading: Boolean = true,
    val errorMessage: String? = null,
    val guildId: String? = null,
    val guildName: String = "",
    val guildIconUrl: String? = null,
    val tracks: List<Track> = emptyList(),
    val isCommandInFlight: Boolean = false,
    val dialog: QueueDialog = QueueDialog.None,
    val addMusic: MAddMusicSheetState = MAddMusicSheetState(),
)
