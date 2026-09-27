package com.dc.melodiasmario.core.network.search.dto

import com.dc.melodiasmario.core.common.TrackSearchPrefix
import com.dc.melodiasmario.core.model.search.MusicSearchResultKind

internal fun String.toTrackSearchPrefixOrNull(): TrackSearchPrefix? {
    return when (lowercase()) {
        "sptfy", "spotify" -> TrackSearchPrefix.SPOTIFY
        "scsearch", "soundcloud" -> TrackSearchPrefix.SOUNDCLOUD
        "ytmsearch", "youtubemusic", "youtube_music", "youtube-music" -> TrackSearchPrefix.YOUTUBE_MUSIC
        "ytsearch", "youtube" -> TrackSearchPrefix.YOUTUBE
        "amsearch", "applemusic", "apple_music", "apple-music" -> TrackSearchPrefix.APPLE_MUSIC
        "dzsearch", "deezer" -> TrackSearchPrefix.DEEZER
        "ymsearch", "yandexmusic", "yandex_music", "yandex-music" -> TrackSearchPrefix.YANDEX_MUSIC
        "bcsearch", "bandcamp" -> TrackSearchPrefix.BANDCAMP
        else -> TrackSearchPrefix.entries.firstOrNull {
            it.name.equals(this, ignoreCase = true)
        }
    }
}

internal fun String.toMusicSearchResultKindOrNull(): MusicSearchResultKind? {
    return when (lowercase()) {
        "track" -> MusicSearchResultKind.TRACK
        "playlist" -> MusicSearchResultKind.PLAYLIST
        else -> MusicSearchResultKind.entries.firstOrNull { it.name.equals(this, ignoreCase = true) }
    }
}