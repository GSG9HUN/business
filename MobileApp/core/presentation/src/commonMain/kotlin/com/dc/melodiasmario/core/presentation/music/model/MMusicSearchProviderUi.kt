package com.dc.melodiasmario.core.presentation.music.model

/**
 * Represents music search provider ui shared between application layers.
 *
 * @property id Id.
 * @property name Name.
 * @property canEnqueue Can enqueue.
 * @property supportsTrackSearch Supports track search.
 * @property supportsPlaylistSearch Supports playlist search.
 * @property unavailableReason Unavailable reason.
 */
data class MMusicSearchProviderUi(
    val id: String,
    val name: String,
    val canEnqueue: Boolean,
    val supportsTrackSearch: Boolean,
    val supportsPlaylistSearch: Boolean,
    val unavailableReason: String? = null
)
