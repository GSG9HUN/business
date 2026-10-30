package com.dc.melodiasmario.core.model.search

import com.dc.melodiasmario.core.common.TrackSearchPrefix

/**
 * Represents music search capability shared between application layers.
 *
 * @property provider Provider.
 * @property supportsTrackSearch Supports track search.
 * @property supportsPlaylistSearch Supports playlist search.
 * @property canEnqueue Can enqueue.
 * @property unavailableReason Unavailable reason.
 */
data class MusicSearchCapability(
    val provider: TrackSearchPrefix?,
    val supportsTrackSearch: Boolean,
    val supportsPlaylistSearch: Boolean,
    val canEnqueue: Boolean,
    val unavailableReason: String? = null
)
