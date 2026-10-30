package com.dc.melodiasmario.feature.queue.presentation

import com.dc.melodiasmario.core.model.currenttrack.Track
import com.dc.melodiasmario.core.presentation.music.search.MusicSearchUiState
import com.dc.melodiasmario.core.presentation.music.state.MAddMusicSheetState
import com.dc.melodiasmario.core.presentation.queue.TrackQueueUiState

/**
 * Defines queue presentation state shared with UI consumers.
 *
 * @property isLoading Is loading.
 * @property errorMessage Error message.
 * @property guildId Identifier of the guild owning this value.
 * @property guildName Guild name.
 * @property guildIconUrl Guild icon url.
 * @property tracks Tracks.
 * @property isCommandInFlight Is command in flight.
 * @property dialog Dialog.
 * @property playlistNameDraft Playlist name draft.
 * @property addMusic Add music.
 */
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
