package com.dc.melodiasmario.core.presentation.music.model

/**
 * Represents music search result ui shared between application layers.
 *
 * @property id Id.
 * @property providerId Provider id.
 * @property providerName Provider name.
 * @property kind Kind.
 * @property title Title.
 * @property creator Creator.
 * @property thumbnailUrl Optional track artwork URL.
 * @property durationText Duration text.
 * @property itemCount Item count.
 * @property canonicalUrl Canonical url.
 * @property canEnqueue Can enqueue.
 */
data class MMusicSearchResultUi(
    val id: String,
    val providerId: String,
    val providerName: String,
    val kind: MMusicSearchKind,
    val title: String,
    val creator: String? = null,
    val thumbnailUrl: String? = null,
    val durationText: String? = null,
    val itemCount: Int? = null,
    val canonicalUrl: String? = null,
    val canEnqueue: Boolean = canonicalUrl != null
)
