package com.dc.melodiasmario.feature.queue.presentation

import com.dc.melodiasmario.core.commonui.music.model.MAddMusicMode
import com.dc.melodiasmario.core.commonui.music.model.MMusicSearchKind

sealed interface QueueEvent {
    data class LoadQueue(val guildId: String) : QueueEvent
    data object RefreshClicked : QueueEvent
    data object AddMusicClicked : QueueEvent
    data object MoreClicked : QueueEvent
    data object DialogDismissed : QueueEvent
    data object ClearQueueClicked : QueueEvent
    data object ShuffleQueueClicked : QueueEvent
    data object MakePlaylistFromQueueClicked : QueueEvent
    data class RemoveTrackClicked(
        val trackId: String,
        val index: Int,
    ) : QueueEvent
    data class MoveTrackClicked(
        val trackId: String,
        val fromIndex: Int,
        val toIndex: Int,
    ) : QueueEvent
    data class AddMusicModeChanged(val mode: MAddMusicMode) : QueueEvent
    data class ManualDraftChanged(val value: String) : QueueEvent
    data object ManualAddClicked : QueueEvent
    data class SearchQueryChanged(val query: String) : QueueEvent
    data object SearchSubmitted : QueueEvent
    data class SearchProviderChanged(val providerId: String) : QueueEvent
    data class SearchKindChanged(val kind: MMusicSearchKind) : QueueEvent
    data class SearchResultClicked(val resultId: String) : QueueEvent
    data object SelectedSearchResultAddClicked : QueueEvent
    data object SearchNextPageRequested : QueueEvent
    data object SearchRetryClicked : QueueEvent
    data object SearchCapabilitiesRetryClicked : QueueEvent
}
