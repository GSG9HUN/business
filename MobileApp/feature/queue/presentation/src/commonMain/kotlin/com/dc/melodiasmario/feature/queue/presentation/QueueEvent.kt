package com.dc.melodiasmario.feature.queue.presentation

import com.dc.melodiasmario.core.presentation.music.model.MAddMusicMode
import com.dc.melodiasmario.core.presentation.music.model.MMusicSearchKind

/**
 * Defines the interactions handled by queue presentation logic.
 */
sealed interface QueueEvent {
    /**
     * Describes the load queue variant of [QueueEvent].
     *
     * @property guildId Identifier of the guild owning this value.
     */
    data class LoadQueue(val guildId: String) : QueueEvent

    /**
     * Describes the refresh clicked variant of [QueueEvent].
     */
    data object RefreshClicked : QueueEvent

    /**
     * Describes the add music clicked variant of [QueueEvent].
     */
    data object AddMusicClicked : QueueEvent

    /**
     * Describes the more clicked variant of [QueueEvent].
     */
    data object MoreClicked : QueueEvent

    /**
     * Describes the dialog dismissed variant of [QueueEvent].
     */
    data object DialogDismissed : QueueEvent

    /**
     * Describes the clear queue clicked variant of [QueueEvent].
     */
    data object ClearQueueClicked : QueueEvent

    /**
     * Describes the shuffle queue clicked variant of [QueueEvent].
     */
    data object ShuffleQueueClicked : QueueEvent

    /**
     * Describes the make playlist from queue clicked variant of [QueueEvent].
     */
    data object MakePlaylistFromQueueClicked : QueueEvent

    /**
     * Describes the playlist name changed variant of [QueueEvent].
     *
     * @property value Value.
     */
    data class PlaylistNameChanged(val value: String) : QueueEvent

    /**
     * Describes the create playlist confirmed variant of [QueueEvent].
     */
    data object CreatePlaylistConfirmed : QueueEvent

    /**
     * Describes the guild clicked variant of [QueueEvent].
     */
    data object GuildClicked : QueueEvent

    /**
     * Describes the remove track clicked variant of [QueueEvent].
     *
     * @property trackId Identifier of the track.
     * @property index Index.
     */
    data class RemoveTrackClicked(
        val trackId: String,
        val index: Int
    ) : QueueEvent

    /**
     * Describes the move track clicked variant of [QueueEvent].
     *
     * @property trackId Identifier of the track.
     * @property fromIndex From index.
     * @property toIndex To index.
     */
    data class MoveTrackClicked(
        val trackId: String,
        val fromIndex: Int,
        val toIndex: Int
    ) : QueueEvent

    /**
     * Describes the add music mode changed variant of [QueueEvent].
     *
     * @property mode Mode.
     */
    data class AddMusicModeChanged(val mode: MAddMusicMode) : QueueEvent

    /**
     * Describes the manual draft changed variant of [QueueEvent].
     *
     * @property value Value.
     */
    data class ManualDraftChanged(val value: String) : QueueEvent

    /**
     * Describes the manual add clicked variant of [QueueEvent].
     */
    data object ManualAddClicked : QueueEvent

    /**
     * Describes the search query changed variant of [QueueEvent].
     *
     * @property query Query.
     */
    data class SearchQueryChanged(val query: String) : QueueEvent

    /**
     * Describes the search submitted variant of [QueueEvent].
     */
    data object SearchSubmitted : QueueEvent

    /**
     * Describes the search provider changed variant of [QueueEvent].
     *
     * @property providerId Provider id.
     */
    data class SearchProviderChanged(val providerId: String) : QueueEvent

    /**
     * Describes the search kind changed variant of [QueueEvent].
     *
     * @property kind Kind.
     */
    data class SearchKindChanged(val kind: MMusicSearchKind) : QueueEvent

    /**
     * Describes the search result clicked variant of [QueueEvent].
     *
     * @property resultId Result id.
     */
    data class SearchResultClicked(val resultId: String) : QueueEvent

    /**
     * Describes the selected search result add clicked variant of [QueueEvent].
     */
    data object SelectedSearchResultAddClicked : QueueEvent

    /**
     * Describes the search next page requested variant of [QueueEvent].
     */
    data object SearchNextPageRequested : QueueEvent

    /**
     * Describes the search retry clicked variant of [QueueEvent].
     */
    data object SearchRetryClicked : QueueEvent

    /**
     * Describes the search capabilities retry clicked variant of [QueueEvent].
     */
    data object SearchCapabilitiesRetryClicked : QueueEvent
}

/**
 * Reports whether command event.
 */
fun QueueEvent.isCommandEvent(): Boolean {
    return when (this) {
        QueueEvent.ManualAddClicked,
        QueueEvent.SelectedSearchResultAddClicked,
        QueueEvent.ClearQueueClicked,
        QueueEvent.ShuffleQueueClicked,
        QueueEvent.CreatePlaylistConfirmed,
        is QueueEvent.RemoveTrackClicked,
        is QueueEvent.MoveTrackClicked -> true

        else -> false
    }
}
