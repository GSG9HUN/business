package com.dc.melodiasmario.feature.currenttrack.presentation

import com.dc.melodiasmario.core.presentation.music.model.MAddMusicMode
import com.dc.melodiasmario.core.presentation.music.model.MMusicSearchKind

/**
 * Defines the interactions handled by current track presentation logic.
 */
sealed interface CurrentTrackEvent {

    /** Opens the guild audio-effect destination from the shortcut below the player card. */
    data object AudioEffectsClicked : CurrentTrackEvent

    /**
     * Submits one absolute volume change for the guild displayed when the gesture completed.
     *
     * @property guildId Guild owning the displayed volume control.
     * @property volumePercent Requested server output percentage in the inclusive range 0-200.
     */
    data class VolumeRequested(val guildId: String, val volumePercent: Int) : CurrentTrackEvent

    /**
     * Commits one completed seek gesture for the attempt captured when dragging started.
     *
     * @property positionSeconds Absolute position before the end of the track.
     * @property expectedPlaybackOperationId Playback attempt to which the gesture belongs.
     */
    data class SeekRequested(
        val positionSeconds: Int,
        val expectedPlaybackOperationId: String
    ) : CurrentTrackEvent

    /**
     * Describes the load current track variant of [CurrentTrackEvent].
     *
     * @property guildId Identifier of the guild owning this value.
     */
    data class LoadCurrentTrack(val guildId: String) : CurrentTrackEvent

    /**
     * Describes the sync current track variant of [CurrentTrackEvent].
     */
    data object SyncCurrentTrack : CurrentTrackEvent

    /**
     * Describes the guild clicked variant of [CurrentTrackEvent].
     */
    data object GuildClicked : CurrentTrackEvent

    /**
     * Describes the add to queue clicked variant of [CurrentTrackEvent].
     */
    data object AddToQueueClicked : CurrentTrackEvent

    /**
     * Describes the add to queue draft changed variant of [CurrentTrackEvent].
     *
     * @property value Value.
     */
    data class AddToQueueDraftChanged(val value: String) : CurrentTrackEvent

    /**
     * Describes the add to queue confirmed variant of [CurrentTrackEvent].
     */
    data object AddToQueueConfirmed : CurrentTrackEvent

    /**
     * Describes the dialog dismissed variant of [CurrentTrackEvent].
     */
    data object DialogDismissed : CurrentTrackEvent

    /**
     * Describes the refresh clicked variant of [CurrentTrackEvent].
     */
    data object RefreshClicked : CurrentTrackEvent

    /**
     * Describes the remove from queue clicked variant of [CurrentTrackEvent].
     *
     * @property trackId Identifier of the track.
     * @property index Index.
     */
    data class RemoveFromQueueClicked(val trackId: String, val index: Int) : CurrentTrackEvent

    /**
     * Describes the clear queue clicked variant of [CurrentTrackEvent].
     */
    data object ClearQueueClicked : CurrentTrackEvent

    /**
     * Describes the more clicked variant of [CurrentTrackEvent].
     */
    data object MoreClicked : CurrentTrackEvent

    /**
     * Describes the profile clicked variant of [CurrentTrackEvent].
     */
    data object ProfileClicked : CurrentTrackEvent

    /**
     * Describes the next clicked variant of [CurrentTrackEvent].
     */
    data object NextClicked : CurrentTrackEvent

    /**
     * Describes the previous clicked variant of [CurrentTrackEvent].
     */
    data object PreviousClicked : CurrentTrackEvent

    /**
     * Describes the repeat clicked variant of [CurrentTrackEvent].
     */
    data object RepeatClicked : CurrentTrackEvent

    /**
     * Describes the shuffle clicked variant of [CurrentTrackEvent].
     */
    data object ShuffleClicked : CurrentTrackEvent

    /**
     * Describes the play pause clicked variant of [CurrentTrackEvent].
     */
    data object PlayPauseClicked : CurrentTrackEvent

    /**
     * Describes the move to index clicked variant of [CurrentTrackEvent].
     *
     * @property trackId Identifier of the track.
     * @property fromIndex From index.
     * @property toIndex To index.
     */
    data class MoveToIndexClicked(
        val trackId: String,
        val fromIndex: Int,
        val toIndex: Int
    ) : CurrentTrackEvent

    /**
     * Describes the search capabilities retry clicked variant of [CurrentTrackEvent].
     */
    data object SearchCapabilitiesRetryClicked : CurrentTrackEvent

    /**
     * Describes the search query changed variant of [CurrentTrackEvent].
     *
     * @property query Query.
     */
    data class SearchQueryChanged(val query: String) : CurrentTrackEvent

    /**
     * Describes the search submitted variant of [CurrentTrackEvent].
     */
    data object SearchSubmitted : CurrentTrackEvent

    /**
     * Describes the search retry clicked variant of [CurrentTrackEvent].
     */
    data object SearchRetryClicked : CurrentTrackEvent

    /**
     * Describes the search next page requested variant of [CurrentTrackEvent].
     */
    data object SearchNextPageRequested : CurrentTrackEvent

    /**
     * Describes the selected search result add clicked variant of [CurrentTrackEvent].
     */
    data object SelectedSearchResultAddClicked : CurrentTrackEvent

    /**
     * Describes the search provider changed variant of [CurrentTrackEvent].
     *
     * @property providerId Provider id.
     */
    data class SearchProviderChanged(val providerId: String) : CurrentTrackEvent

    /**
     * Describes the search kind changed variant of [CurrentTrackEvent].
     *
     * @property kind Kind.
     */
    data class SearchKindChanged(val kind: MMusicSearchKind) : CurrentTrackEvent

    /**
     * Describes the search result clicked variant of [CurrentTrackEvent].
     *
     * @property resultId Result id.
     */
    data class SearchResultClicked(val resultId: String) : CurrentTrackEvent

    /**
     * Describes the add music mode changed variant of [CurrentTrackEvent].
     *
     * @property mode Mode.
     */
    data class AddMusicModeChanged(val mode: MAddMusicMode) : CurrentTrackEvent
}

/**
 * Reports whether command event.
 */
fun CurrentTrackEvent.isCommandEvent(): Boolean {
    return when (this) {
        CurrentTrackEvent.AddToQueueConfirmed,
        is CurrentTrackEvent.RemoveFromQueueClicked,
        CurrentTrackEvent.ClearQueueClicked,
        CurrentTrackEvent.NextClicked,
        is CurrentTrackEvent.SeekRequested,
        is CurrentTrackEvent.VolumeRequested,
        CurrentTrackEvent.PreviousClicked,
        CurrentTrackEvent.RepeatClicked,
        CurrentTrackEvent.ShuffleClicked,
        CurrentTrackEvent.PlayPauseClicked,
        is CurrentTrackEvent.MoveToIndexClicked,
        CurrentTrackEvent.SelectedSearchResultAddClicked -> true

        else -> false
    }
}
