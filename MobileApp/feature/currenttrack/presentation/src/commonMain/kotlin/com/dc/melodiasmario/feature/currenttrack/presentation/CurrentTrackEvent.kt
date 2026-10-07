package com.dc.melodiasmario.feature.currenttrack.presentation

import com.dc.melodiasmario.core.commonui.music.model.MAddMusicMode
import com.dc.melodiasmario.core.commonui.music.model.MMusicSearchKind

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
    data class SearchQueryChanged(val query: String) : CurrentTrackEvent
    data object SearchSubmitted : CurrentTrackEvent
    data object SearchRetryClicked : CurrentTrackEvent
    data object SearchNextPageRequested : CurrentTrackEvent
    data object SelectedSearchResultAddClicked : CurrentTrackEvent

    data class SearchProviderChanged(val providerId: String) : CurrentTrackEvent
    data class SearchKindChanged(val kind: MMusicSearchKind) : CurrentTrackEvent
    data class SearchResultClicked(val resultId: String) : CurrentTrackEvent
    data class AddMusicModeChanged(val mode: MAddMusicMode) : CurrentTrackEvent
}
