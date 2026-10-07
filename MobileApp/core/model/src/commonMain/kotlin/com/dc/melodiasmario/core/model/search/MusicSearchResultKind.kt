package com.dc.melodiasmario.core.model.search

enum class MusicSearchResultKind {
    TRACK,
    PLAYLIST;

    fun toApiKind(): String {
        return when (this) {
            MusicSearchResultKind.TRACK -> "track"
            MusicSearchResultKind.PLAYLIST -> "playlist"
        }
    }
}