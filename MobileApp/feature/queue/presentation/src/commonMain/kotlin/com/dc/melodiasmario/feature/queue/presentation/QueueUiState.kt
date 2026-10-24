package com.dc.melodiasmario.feature.queue.presentation

import com.dc.melodiasmario.core.presentation.queue.TrackQueueUiState
import com.dc.melodiasmario.core.presentation.music.search.MusicSearchUiState
import com.dc.melodiasmario.core.presentation.music.state.MAddMusicSheetState
import com.dc.melodiasmario.core.model.currenttrack.Track

data class QueueUiState(
    val isLoading: Boolean = true,
    val errorMessage: String? = null,
    val guildId: String? = null,
    val guildName: String = "",
    val guildIconUrl: String? = null,
    override val tracks: List<Track> = emptyList(),
    override val isCommandInFlight: Boolean = false,
    val dialog: QueueDialog = QueueDialog.None,
    val playlistNameDraft: String = "",
    override val addMusic: MAddMusicSheetState = MAddMusicSheetState()
) : MusicSearchUiState<QueueUiState>, TrackQueueUiState<Track, QueueUiState> {

    override fun withCommandInFlight(isCommandInFlight: Boolean): QueueUiState {
        return copy(
            isCommandInFlight = isCommandInFlight,
            addMusic = addMusic.copy(commandInFlight = isCommandInFlight)
        )
    }

    override fun withTracks(tracks: List<Track>): QueueUiState {
        return copy(tracks = tracks)
    }

    override fun withAddMusic(addMusic: MAddMusicSheetState): QueueUiState {
        return copy(addMusic = addMusic)
    }
}
