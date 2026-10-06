package com.dc.melodiasmario.feature.currenttrack.presentation.search

import com.dc.melodiasmario.core.common.TrackSearchPrefix
import com.dc.melodiasmario.core.commonui.formatter.toDurationLabel
import com.dc.melodiasmario.core.commonui.music.model.MMusicSearchKind
import com.dc.melodiasmario.core.commonui.music.model.MMusicSearchProviderUi
import com.dc.melodiasmario.core.commonui.music.model.MMusicSearchResultUi
import com.dc.melodiasmario.core.model.search.MusicSearchCapability
import com.dc.melodiasmario.core.model.search.MusicSearchResult
import com.dc.melodiasmario.core.model.search.MusicSearchResultKind

internal fun MusicSearchCapability.toProviderUi(): MMusicSearchProviderUi? {
    val provider = provider ?: return null
    return MMusicSearchProviderUi(
        id = provider.toMusicSearchProviderId(),
        name = provider.displayName(),
        canEnqueue = canEnqueue,
        supportsTrackSearch = supportsTrackSearch,
        supportsPlaylistSearch = supportsPlaylistSearch,
        unavailableReason = unavailableReason,
    )
}

internal fun MusicSearchResult.toResultUi(): MMusicSearchResultUi {
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
        canEnqueue = canEnqueue,
    )
}

internal fun String.toTrackSearchPrefixOrDefault(): TrackSearchPrefix {
    return TrackSearchPrefix.entries.firstOrNull { it.toMusicSearchProviderId() == this }
        ?: TrackSearchPrefix.YOUTUBE
}

internal fun MMusicSearchKind.toDomainKind(): MusicSearchResultKind {
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
