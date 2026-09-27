package com.dc.melodiasmario.feature.currenttrack.presentation.state

import com.dc.melodiasmario.core.common.TrackSearchPrefix
import com.dc.melodiasmario.core.model.search.MusicSearchCapability
import com.dc.melodiasmario.core.model.search.MusicSearchResult
import com.dc.melodiasmario.core.model.search.MusicSearchResultKind
import com.dc.melodiasmario.feature.currenttrack.presentation.addmusic.CurrentTrackAddMusicMode

data class CurrentTrackSearchUiState(
    val mode: CurrentTrackAddMusicMode = CurrentTrackAddMusicMode.Search,
    val query: String = "",
    val selectedProvider: TrackSearchPrefix = TrackSearchPrefix.YOUTUBE,
    val selectedKind: MusicSearchResultKind = MusicSearchResultKind.TRACK,
    val capabilities: List<MusicSearchCapability> = emptyList(),
    val results: List<MusicSearchResult> = emptyList(),
    val selectedResult: MusicSearchResult? = null,
    val nextPageToken: String? = null,
    val isCapabilitiesLoading: Boolean = false,
    val isSearching: Boolean = false,
    val isLoadingNextPage: Boolean = false,
    val errorMessage: String? = null,
)
