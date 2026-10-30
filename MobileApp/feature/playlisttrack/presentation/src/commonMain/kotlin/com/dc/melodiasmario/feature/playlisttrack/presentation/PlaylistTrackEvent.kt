package com.dc.melodiasmario.feature.playlisttrack.presentation

import com.dc.melodiasmario.core.presentation.music.model.MAddMusicMode
import com.dc.melodiasmario.core.presentation.music.model.MMusicSearchKind

sealed interface PlaylistTrackEvent {
    data class Load(val guildId: String, val playlistId: String) : PlaylistTrackEvent
    data object BackClicked : PlaylistTrackEvent
    data object Refresh : PlaylistTrackEvent
    data object LoadPlaylist : PlaylistTrackEvent
    data object MoreClicked : PlaylistTrackEvent
    data object Shuffle : PlaylistTrackEvent
    data object AddMusicClicked : PlaylistTrackEvent
    data object DismissDialog : PlaylistTrackEvent
    data class TrackMoreClicked(val orderNumber: Int) : PlaylistTrackEvent
    data class RemoveClicked(val orderNumber: Int) : PlaylistTrackEvent
    data class RemoveConfirmed(val orderNumber: Int) : PlaylistTrackEvent
    data class Move(val fromIndex: Int, val toIndex: Int) : PlaylistTrackEvent
    data class ModeChanged(val mode: MAddMusicMode) : PlaylistTrackEvent
    data class ManualDraftChanged(val value: String) : PlaylistTrackEvent
    data object ManualAdd : PlaylistTrackEvent
    data class SearchQueryChanged(val value: String) : PlaylistTrackEvent
    data class ProviderChanged(val id: String) : PlaylistTrackEvent
    data class KindChanged(val kind: MMusicSearchKind) : PlaylistTrackEvent
    data class ResultClicked(val id: String) : PlaylistTrackEvent
    data object AddSelectedResult : PlaylistTrackEvent
    data object SearchSubmitted : PlaylistTrackEvent
    data object NextPage : PlaylistTrackEvent
    data object RetrySearch : PlaylistTrackEvent
    data object RetryCapabilities : PlaylistTrackEvent
}

fun PlaylistTrackEvent.isCommandEvent(): Boolean = when (this) {
    is PlaylistTrackEvent.RemoveConfirmed,
    PlaylistTrackEvent.LoadPlaylist,
    PlaylistTrackEvent.ManualAdd,
    PlaylistTrackEvent.AddSelectedResult -> true

    else -> false
}
