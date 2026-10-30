package com.dc.melodiasmario.feature.playlisttrack.presentation

import com.dc.melodiasmario.core.presentation.music.model.MAddMusicMode
import com.dc.melodiasmario.core.presentation.music.model.MMusicSearchKind

/**
 * Defines the interactions handled by playlist track presentation logic.
 */
sealed interface PlaylistTrackEvent {
    /**
     * Describes the load variant of [PlaylistTrackEvent].
     *
     * @property guildId Identifier of the guild owning this value.
     * @property playlistId Identifier of the saved playlist.
     */
    data class Load(val guildId: String, val playlistId: String) : PlaylistTrackEvent

    /**
     * Describes the back clicked variant of [PlaylistTrackEvent].
     */
    data object BackClicked : PlaylistTrackEvent

    /**
     * Describes the refresh variant of [PlaylistTrackEvent].
     */
    data object Refresh : PlaylistTrackEvent

    /**
     * Describes the load playlist variant of [PlaylistTrackEvent].
     */
    data object LoadPlaylist : PlaylistTrackEvent

    /**
     * Describes the more clicked variant of [PlaylistTrackEvent].
     */
    data object MoreClicked : PlaylistTrackEvent

    /**
     * Describes the shuffle variant of [PlaylistTrackEvent].
     */
    data object Shuffle : PlaylistTrackEvent

    /**
     * Describes the add music clicked variant of [PlaylistTrackEvent].
     */
    data object AddMusicClicked : PlaylistTrackEvent

    /**
     * Describes the dismiss dialog variant of [PlaylistTrackEvent].
     */
    data object DismissDialog : PlaylistTrackEvent

    /**
     * Describes the track more clicked variant of [PlaylistTrackEvent].
     *
     * @property orderNumber Stored order number of the playlist track.
     */
    data class TrackMoreClicked(val orderNumber: Int) : PlaylistTrackEvent

    /**
     * Describes the remove clicked variant of [PlaylistTrackEvent].
     *
     * @property orderNumber Stored order number of the playlist track.
     */
    data class RemoveClicked(val orderNumber: Int) : PlaylistTrackEvent

    /**
     * Describes the remove confirmed variant of [PlaylistTrackEvent].
     *
     * @property orderNumber Stored order number of the playlist track.
     */
    data class RemoveConfirmed(val orderNumber: Int) : PlaylistTrackEvent

    /**
     * Describes the move variant of [PlaylistTrackEvent].
     *
     * @property fromIndex From index.
     * @property toIndex To index.
     */
    data class Move(val fromIndex: Int, val toIndex: Int) : PlaylistTrackEvent

    /**
     * Describes the mode changed variant of [PlaylistTrackEvent].
     *
     * @property mode Mode.
     */
    data class ModeChanged(val mode: MAddMusicMode) : PlaylistTrackEvent

    /**
     * Describes the manual draft changed variant of [PlaylistTrackEvent].
     *
     * @property value Value.
     */
    data class ManualDraftChanged(val value: String) : PlaylistTrackEvent

    /**
     * Describes the manual add variant of [PlaylistTrackEvent].
     */
    data object ManualAdd : PlaylistTrackEvent

    /**
     * Describes the search query changed variant of [PlaylistTrackEvent].
     *
     * @property value Value.
     */
    data class SearchQueryChanged(val value: String) : PlaylistTrackEvent

    /**
     * Describes the provider changed variant of [PlaylistTrackEvent].
     *
     * @property id Id.
     */
    data class ProviderChanged(val id: String) : PlaylistTrackEvent

    /**
     * Describes the kind changed variant of [PlaylistTrackEvent].
     *
     * @property kind Kind.
     */
    data class KindChanged(val kind: MMusicSearchKind) : PlaylistTrackEvent

    /**
     * Describes the result clicked variant of [PlaylistTrackEvent].
     *
     * @property id Id.
     */
    data class ResultClicked(val id: String) : PlaylistTrackEvent

    /**
     * Describes the add selected result variant of [PlaylistTrackEvent].
     */
    data object AddSelectedResult : PlaylistTrackEvent

    /**
     * Describes the search submitted variant of [PlaylistTrackEvent].
     */
    data object SearchSubmitted : PlaylistTrackEvent

    /**
     * Describes the next page variant of [PlaylistTrackEvent].
     */
    data object NextPage : PlaylistTrackEvent

    /**
     * Describes the retry search variant of [PlaylistTrackEvent].
     */
    data object RetrySearch : PlaylistTrackEvent

    /**
     * Describes the retry capabilities variant of [PlaylistTrackEvent].
     */
    data object RetryCapabilities : PlaylistTrackEvent
}

/**
 * Reports whether command event.
 */
fun PlaylistTrackEvent.isCommandEvent(): Boolean = when (this) {
    is PlaylistTrackEvent.RemoveConfirmed,
    PlaylistTrackEvent.LoadPlaylist,
    PlaylistTrackEvent.ManualAdd,
    PlaylistTrackEvent.AddSelectedResult -> true

    else -> false
}
