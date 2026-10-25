package com.dc.melodiasmario.feature.currenttrack.presentation.state

import com.dc.melodiasmario.core.presentation.queue.TrackQueueUiState
import com.dc.melodiasmario.core.presentation.music.search.MusicSearchUiState
import com.dc.melodiasmario.core.presentation.music.state.MAddMusicSheetState
import com.dc.melodiasmario.core.model.currenttrack.Track
import com.dc.melodiasmario.feature.currenttrack.presentation.dialog.CurrentTrackDialog

data class CurrentTrackUiState(
    val isLoading: Boolean = false,
    val errorMessage: String? = null,
    val header: CurrentTrackHeaderUiState = CurrentTrackHeaderUiState(),
    val playback: CurrentTrackPlaybackUiState = CurrentTrackPlaybackUiState(),
    val queue: CurrentTrackQueueUiState = CurrentTrackQueueUiState(),
    val actions: CurrentTrackActionsUiState = CurrentTrackActionsUiState(),
    override val addMusic: MAddMusicSheetState = MAddMusicSheetState(),
    val dialog: CurrentTrackDialog? = null
) : MusicSearchUiState<CurrentTrackUiState>, TrackQueueUiState<Track, CurrentTrackUiState> {

    override val isCommandInFlight: Boolean
        get() = actions.isCommandInFlight

    override val tracks: List<Track>
        get() = queue.tracks

    override fun withCommandInFlight(isCommandInFlight: Boolean): CurrentTrackUiState {
        return copy(
            actions = actions.copy(isCommandInFlight = isCommandInFlight),
            addMusic = addMusic.copy(commandInFlight = isCommandInFlight)
        )
    }

    override fun withTracks(tracks: List<Track>): CurrentTrackUiState {
        return copy(queue = queue.copy(tracks = tracks))
    }

    override fun withAddMusic(addMusic: MAddMusicSheetState): CurrentTrackUiState {
        return copy(addMusic = addMusic)
    }
}





