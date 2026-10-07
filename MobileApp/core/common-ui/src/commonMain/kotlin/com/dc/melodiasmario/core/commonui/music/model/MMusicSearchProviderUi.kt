package com.dc.melodiasmario.core.commonui.music.model

data class MMusicSearchProviderUi(
    val id: String,
    val name: String,
    val canEnqueue: Boolean,
    val supportsTrackSearch: Boolean,
    val supportsPlaylistSearch: Boolean,
    val unavailableReason: String? = null,
)