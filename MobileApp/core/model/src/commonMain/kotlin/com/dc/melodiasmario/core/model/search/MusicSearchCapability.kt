package com.dc.melodiasmario.core.model.search

import com.dc.melodiasmario.core.common.TrackSearchPrefix

data class MusicSearchCapability(
    val provider: TrackSearchPrefix?,
    val supportsTrackSearch: Boolean,
    val supportsPlaylistSearch: Boolean,
    val canEnqueue: Boolean,
    val unavailableReason: String? = null,
)