package com.dc.melodiasmario.feature.currenttrack.presentation

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

    data class MoveUpClicked(val trackId: String, val index: Int) : CurrentTrackEvent
    data class MoveDownClicked(val trackId: String, val index: Int) : CurrentTrackEvent
}
