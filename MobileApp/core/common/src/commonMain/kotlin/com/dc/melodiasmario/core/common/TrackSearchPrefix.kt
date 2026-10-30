package com.dc.melodiasmario.core.common

/**
 * Search prefixes supported by the bot's TrackSearchResolverService, without the colon.
 *
 * @property prefix Prefix.
 * @property alias Alias.
 */
enum class TrackSearchPrefix(val prefix: String, val alias: String) {
    SPOTIFY(prefix = "sptfy", alias = "spotify"),
    SOUNDCLOUD(prefix = "scsearch", alias = "soundcloud"),
    YOUTUBE_MUSIC(prefix = "ytmsearch", alias = "youtubemusic"),
    YOUTUBE(prefix = "ytsearch", alias = "youtube"),
    APPLE_MUSIC(prefix = "amsearch", alias = "applemusic"),
    DEEZER(prefix = "dzsearch", alias = "deezer"),
    YANDEX_MUSIC(prefix = "ymsearch", alias = "yandexmusic"),
    BANDCAMP(prefix = "bcsearch", alias = "bandcamp");

    /**
     * Maps this value to music search provider id.
     */
    fun toMusicSearchProviderId(): String {
        return when (this) {
            SPOTIFY -> "sptfy"
            SOUNDCLOUD -> "scsearch"
            YOUTUBE_MUSIC -> "ytmsearch"
            YOUTUBE -> "ytsearch"
            APPLE_MUSIC -> "amsearch"
            DEEZER -> "dzsearch"
            YANDEX_MUSIC -> "ymsearch"
            BANDCAMP -> "bcsearch"
        }
    }

    /**
     * Shared values and operations for the enclosing type.
     */
    companion object {
        /** Returns null for unprefixed queries, URLs, and unsupported prefixes. */
        fun fromInput(input: String): TrackSearchPrefix? {
            if (':' !in input) {
                return null
            }

            val value = input.substringBefore(':')
            return entries.firstOrNull { searchPrefix ->
                searchPrefix.prefix.equals(other = value, ignoreCase = true) ||
                        searchPrefix.alias.equals(other = value, ignoreCase = true)
            }
        }
    }
}
