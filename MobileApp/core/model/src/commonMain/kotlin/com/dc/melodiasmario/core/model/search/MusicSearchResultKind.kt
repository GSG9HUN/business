package com.dc.melodiasmario.core.model.search

/**
 * Represents music search result kind shared between application layers.
 */
enum class MusicSearchResultKind {
    TRACK,
    PLAYLIST;

    /**
     * Maps this value to api kind.
     */
    fun toApiKind(): String {
        return when (this) {
            TRACK -> "track"
            PLAYLIST -> "playlist"
        }
    }
}
