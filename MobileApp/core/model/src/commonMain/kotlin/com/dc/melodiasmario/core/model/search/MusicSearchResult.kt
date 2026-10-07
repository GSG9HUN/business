package com.dc.melodiasmario.core.model.search

import com.dc.melodiasmario.core.common.TrackSearchPrefix

data class MusicSearchResult(
    val id: String,
    val provider: TrackSearchPrefix,
    val kind: MusicSearchResultKind,
    val title: String,
    val creator: String? = null,
    val thumbnailUrl: String? = null,
    val durationSeconds: Int? = null,
    val itemCount: Int? = null,
    val canonicalUrl: String?,
    val canEnqueue: Boolean = canonicalUrl != null,
)