package com.dc.melodiasmario.core.model.search

/**
 * Represents music search page shared between application layers.
 *
 * @property results Results.
 * @property nextPageToken Next page token.
 */
data class MusicSearchPage(
    val results: List<MusicSearchResult>,
    val nextPageToken: String? = null
)
