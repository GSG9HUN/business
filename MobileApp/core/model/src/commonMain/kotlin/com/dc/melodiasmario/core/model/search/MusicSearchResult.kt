package com.dc.melodiasmario.core.model.search

import com.dc.melodiasmario.core.common.TrackSearchPrefix

/**
 * Represents music search result shared between application layers.
 *
 * @property id Id.
 * @property provider Provider.
 * @property kind Kind.
 * @property title Title.
 * @property creator Creator.
 * @property thumbnailUrl Optional track artwork URL.
 * @property durationSeconds Track duration in seconds.
 * @property itemCount Item count.
 * @property canonicalUrl Canonical url.
 * @property canEnqueue Can enqueue.
 */
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
    val canEnqueue: Boolean = canonicalUrl != null
)
