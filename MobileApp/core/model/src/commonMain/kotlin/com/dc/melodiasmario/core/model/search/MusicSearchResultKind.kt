package com.dc.melodiasmario.core.model.search

enum class MusicSearchResultKind {
    TRACK,
    PLAYLIST;

    fun toApiKind(): String {
        return when (this) {
            TRACK -> "track"
            PLAYLIST -> "playlist"
        }
    }
}