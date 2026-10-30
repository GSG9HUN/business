package com.dc.melodiasmario.core.presentation.music.search

import com.dc.melodiasmario.core.presentation.command.CommandUiState
import com.dc.melodiasmario.core.presentation.music.state.MAddMusicSheetState

/**
 * Defines music search presentation state shared with UI consumers.
 */
interface MusicSearchUiState<State> : CommandUiState<State> {
    /**
     * Add music exposed by music search ui state.
     */
    val addMusic: MAddMusicSheetState

    /**
     * Returns state containing the supplied music-input sheet state.
     */
    fun withAddMusic(addMusic: MAddMusicSheetState): State
}
