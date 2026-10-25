package com.dc.melodiasmario.core.presentation.music.search

import com.dc.melodiasmario.core.presentation.command.CommandUiState
import com.dc.melodiasmario.core.presentation.music.state.MAddMusicSheetState

interface MusicSearchUiState<State> : CommandUiState<State> {
    val addMusic: MAddMusicSheetState

    fun withAddMusic(
        addMusic: MAddMusicSheetState,
    ): State
}