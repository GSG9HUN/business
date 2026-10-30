package com.dc.melodiasmario.core.presentation.music

import com.dc.melodiasmario.core.common.TrackSearchPrefix
import com.dc.melodiasmario.core.presentation.formatter.toDurationLabel
import com.dc.melodiasmario.core.presentation.music.model.MMusicSearchKind
import com.dc.melodiasmario.core.presentation.music.model.MMusicSearchProviderUi
import com.dc.melodiasmario.core.presentation.music.model.MMusicSearchResultUi
import com.dc.melodiasmario.core.model.search.MusicSearchCapability
import com.dc.melodiasmario.core.model.search.MusicSearchResult
import com.dc.melodiasmario.core.model.search.MusicSearchResultKind

fun MusicSearchCapability.toProviderUi(): MMusicSearchProviderUi? {
    val provider = provider ?: return null
    return MMusicSearchProviderUi(
        id = provider.toMusicSearchProviderId(),
        name = provider.displayName(),
        canEnqueue = canEnqueue,
        supportsTrackSearch = supportsTrackSearch,
        supportsPlaylistSearch = supportsPlaylistSearch,
        unavailableReason = unavailableReason
    )
}

fun MusicSearchResult.toResultUi(): MMusicSearchResultUi {
    return MMusicSearchResultUi(
        id = id,
        providerId = provider.toMusicSearchProviderId(),
        providerName = provider.displayName(),
        kind = kind.toUiKind(),
        title = title,
        creator = creator,
        thumbnailUrl = thumbnailUrl,
        durationText = durationSeconds?.toDurationLabel(),
        itemCount = itemCount,
        canonicalUrl = canonicalUrl,
        canEnqueue = canEnqueue
    )
}

fun String.toTrackSearchPrefixOrDefault(): TrackSearchPrefix {
    return TrackSearchPrefix.entries.firstOrNull { it.toMusicSearchProviderId() == this }
        ?: TrackSearchPrefix.YOUTUBE
}

fun MMusicSearchKind.toDomainKind(): MusicSearchResultKind {
    return when (this) {
        MMusicSearchKind.Track -> MusicSearchResultKind.TRACK
        MMusicSearchKind.Playlist -> MusicSearchResultKind.PLAYLIST
    }
}

private fun MusicSearchResultKind.toUiKind(): MMusicSearchKind {
    return when (this) {
        MusicSearchResultKind.TRACK -> MMusicSearchKind.Track
        MusicSearchResultKind.PLAYLIST -> MMusicSearchKind.Playlist
    }
}

private fun TrackSearchPrefix.displayName(): String {
    return when (this) {
        TrackSearchPrefix.SPOTIFY -> "Spotify"
        TrackSearchPrefix.SOUNDCLOUD -> "SoundCloud"
        TrackSearchPrefix.YOUTUBE_MUSIC -> "YouTube Music"
        TrackSearchPrefix.YOUTUBE -> "YouTube"
        TrackSearchPrefix.APPLE_MUSIC -> "Apple Music"
        TrackSearchPrefix.DEEZER -> "Deezer"
        TrackSearchPrefix.YANDEX_MUSIC -> "Yandex Music"
        TrackSearchPrefix.BANDCAMP -> "Bandcamp"
    }
}
