package com.dc.melodiasmario.feature.currenttrack.presentation

import com.dc.melodiasmario.core.common.TrackSearchPrefix
import com.dc.melodiasmario.core.model.search.MusicSearchResult
import com.dc.melodiasmario.core.model.search.MusicSearchResultKind
import com.dc.melodiasmario.feature.currenttrack.presentation.addmusic.CurrentTrackAddMusicMode

sealed interface CurrentTrackEvent {
    data class LoadCurrentTrack(val guildId: String) : CurrentTrackEvent
    data object SyncCurrentTrack : CurrentTrackEvent
    data object GuildClicked : CurrentTrackEvent
    data object AddToQueueClicked : CurrentTrackEvent
    data class AddToQueueDraftChanged(val value: String) : CurrentTrackEvent
    data object AddToQueueConfirmed : CurrentTrackEvent
    data object DialogDismissed : CurrentTrackEvent
    data object RefreshClicked : CurrentTrackEvent

    data class RemoveFromQueueClicked(val trackId: String, val index: Int) : CurrentTrackEvent
    data object ClearQueueClicked : CurrentTrackEvent
    data object MoreClicked : CurrentTrackEvent

    data object ProfileClicked : CurrentTrackEvent
    data object NextClicked : CurrentTrackEvent
    data object PreviousClicked : CurrentTrackEvent
    data object RepeatClicked : CurrentTrackEvent
    data object ShuffleClicked : CurrentTrackEvent
    data object PlayPauseClicked : CurrentTrackEvent
    data class MoveToIndexClicked(
        val trackId: String,
        val fromIndex: Int,
        val toIndex: Int,
    ) : CurrentTrackEvent

    data object SearchCapabilitiesRetryClicked : CurrentTrackEvent
    data class AddMusicModeChanged(val mode: CurrentTrackAddMusicMode) : CurrentTrackEvent
    data class SearchProviderChanged(val provider: TrackSearchPrefix) : CurrentTrackEvent
    data class SearchKindChanged(val kind: MusicSearchResultKind) : CurrentTrackEvent
    data class SearchQueryChanged(val query: String) : CurrentTrackEvent
    data object SearchSubmitted : CurrentTrackEvent
    data object SearchRetryClicked : CurrentTrackEvent
    data object SearchNextPageRequested : CurrentTrackEvent
    data class SearchResultClicked(val result: MusicSearchResult) : CurrentTrackEvent
    data object SelectedSearchResultAddClicked : CurrentTrackEvent
}

fun CurrentTrackEvent.isCommandEvent(): Boolean {
    return this is CurrentTrackEvent.AddToQueueConfirmed ||
            this is CurrentTrackEvent.RemoveFromQueueClicked ||
            this is CurrentTrackEvent.ClearQueueClicked ||
            this is CurrentTrackEvent.NextClicked ||
            this is CurrentTrackEvent.PreviousClicked ||
            this is CurrentTrackEvent.RepeatClicked ||
            this is CurrentTrackEvent.ShuffleClicked ||
            this is CurrentTrackEvent.PlayPauseClicked ||
            this is CurrentTrackEvent.MoveToIndexClicked ||
            this is CurrentTrackEvent.SelectedSearchResultAddClicked
}
