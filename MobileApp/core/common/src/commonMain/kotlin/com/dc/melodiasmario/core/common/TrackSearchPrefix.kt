package com.dc.melodiasmario.core.common

/** Search prefixes supported by the bot's TrackSearchResolverService, without the colon. */
enum class TrackSearchPrefix(val prefix: String, val alias: String) {
    SPOTIFY("sptfy", "spotify"),
    SOUNDCLOUD("scsearch", "soundcloud"),
    YOUTUBE_MUSIC("ytmsearch", "youtubemusic"),
    YOUTUBE("ytsearch", "youtube"),
    APPLE_MUSIC("amsearch", "applemusic"),
    DEEZER("dzsearch", "deezer"),
    YANDEX_MUSIC("ymsearch", "yandexmusic"),
    BANDCAMP("bcsearch", "bandcamp");

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

    companion object {
        /** Returns null for unprefixed queries, URLs, and unsupported prefixes. */
        fun fromInput(input: String): TrackSearchPrefix? {
            if (':' !in input) return null
            val value = input.substringBefore(':')
            return entries.firstOrNull {
                it.prefix.equals(value, ignoreCase = true) ||
                        it.alias.equals(value, ignoreCase = true)
            }
        }
    }
}
