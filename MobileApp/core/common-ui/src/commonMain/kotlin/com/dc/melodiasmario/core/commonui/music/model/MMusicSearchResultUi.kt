package com.dc.melodiasmario.core.commonui.music.model

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
    val canEnqueue: Boolean = canonicalUrl != null,
)