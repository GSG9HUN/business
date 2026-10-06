package com.dc.melodiasmario.core.model.search

data class MusicSearchPage(
    val results: List<MusicSearchResult>,
    val nextPageToken: String? = null,
)