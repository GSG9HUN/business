package com.dc.melodiasmario.feature.playlisttrack.presentation

import com.dc.melodiasmario.core.model.playlist.PlaylistDetail
import com.dc.melodiasmario.core.model.playlist.PlaylistTrack
import com.dc.melodiasmario.core.presentation.music.search.MusicSearchUiState
import com.dc.melodiasmario.core.presentation.music.state.MAddMusicSheetState

/**
 * Defines playlist track presentation state shared with UI consumers.
 *
 * @property isLoading Is loading.
 * @property isRefreshing Is refreshing.
 * @property hasLoadError Has load error.
 * @property playlist Playlist.
 * @property dialog Dialog.
 * @property isCommandInFlight Is command in flight.
 * @property addMusic Add music.
 * @property canReorder Can reorder.
 */
data class PlaylistTrackUiState(
    val isLoading: Boolean = true,
    val isRefreshing: Boolean = false,
    val hasLoadError: Boolean = false,
    val playlist: PlaylistDetail? = null,
    val dialog: PlaylistTrackDialog = PlaylistTrackDialog.None,
    override val isCommandInFlight: Boolean = false,
    override val addMusic: MAddMusicSheetState = MAddMusicSheetState(),
    val canReorder: Boolean = false
) : MusicSearchUiState<PlaylistTrackUiState> {
    /**
     * Tracks exposed by playlist track ui state.
     */
    val tracks: List<PlaylistTrack> get() = playlist?.tracks.orEmpty()

    /**
     * Can modify exposed by playlist track ui state.
     */
    val canModify: Boolean get() = playlist != null && !isLoading && !isRefreshing && !isCommandInFlight && !hasLoadError

    override fun withCommandInFlight(isCommandInFlight: Boolean) = copy(
        isCommandInFlight = isCommandInFlight,
        addMusic = addMusic.copy(commandInFlight = isCommandInFlight)
    )

    override fun withAddMusic(addMusic: MAddMusicSheetState) = copy(addMusic = addMusic)
}
