package com.dc.melodiasmario.feature.currenttrack.presentation.search

import com.dc.melodiasmario.core.common.TrackSearchPrefix
import com.dc.melodiasmario.core.model.search.MusicSearchCapability
import com.dc.melodiasmario.core.model.search.MusicSearchPage
import com.dc.melodiasmario.core.model.search.MusicSearchResult
import com.dc.melodiasmario.core.model.search.MusicSearchResultKind
import com.dc.melodiasmario.feature.currenttrack.presentation.addmusic.CurrentTrackAddMusicMode
import com.dc.melodiasmario.feature.currenttrack.presentation.state.CurrentTrackSearchUiState
import com.dc.melodiasmario.feature.currenttrack.presentation.state.CurrentTrackUiState

internal object CurrentTrackSearchReducer {
    fun reset(state: CurrentTrackUiState): CurrentTrackUiState {
        return state.copy(search = CurrentTrackSearchUiState())
    }

    fun queryChanged(state: CurrentTrackUiState, query: String): CurrentTrackUiState {
        return state.copy(
            search = state.search.copy(
                query = query,
                selectedResult = null,
            )
        )
    }

    fun clearShortQueryResults(state: CurrentTrackUiState): CurrentTrackUiState {
        return state.copy(
            search = state.search.copy(
                results = emptyList(),
                nextPageToken = null,
                errorMessage = null,
                isSearching = false,
                isLoadingNextPage = false,
            )
        )
    }

    fun capabilitiesLoading(state: CurrentTrackUiState): CurrentTrackUiState {
        return state.copy(
            search = state.search.copy(
                isCapabilitiesLoading = true,
                errorMessage = null,
            )
        )
    }

    fun capabilitiesLoaded(
        state: CurrentTrackUiState,
        capabilities: List<MusicSearchCapability>,
    ): CurrentTrackUiState {
        return state.copy(
            search = state.search.copy(
                capabilities = capabilities,
                isCapabilitiesLoading = false,
                errorMessage = null,
            )
        )
    }

    fun capabilitiesFailed(
        state: CurrentTrackUiState,
        error: Throwable,
    ): CurrentTrackUiState {
        return state.copy(
            search = state.search.copy(
                isCapabilitiesLoading = false,
                errorMessage = error.message,
            )
        )
    }

    fun providerChanged(
        state: CurrentTrackUiState,
        provider: TrackSearchPrefix,
    ): CurrentTrackUiState {
        return state.copy(
            search = state.search.copy(
                selectedProvider = provider,
                results = emptyList(),
                selectedResult = null,
                nextPageToken = null,
                errorMessage = null,
                isSearching = false,
                isLoadingNextPage = false,
            )
        )
    }

    fun kindChanged(
        state: CurrentTrackUiState,
        kind: MusicSearchResultKind,
    ): CurrentTrackUiState {
        return state.copy(
            search = state.search.copy(
                selectedKind = kind,
                results = emptyList(),
                selectedResult = null,
                nextPageToken = null,
                errorMessage = null,
                isSearching = false,
                isLoadingNextPage = false,
            )
        )
    }

    fun modeChanged(
        state: CurrentTrackUiState,
        mode: CurrentTrackAddMusicMode,
    ): CurrentTrackUiState {
        return state.copy(
            search = state.search.copy(
                mode = mode,
                selectedResult = null,
                errorMessage = null,
            )
        )
    }

    fun resultSelected(
        state: CurrentTrackUiState,
        result: MusicSearchResult,
    ): CurrentTrackUiState {
        return state.copy(search = state.search.copy(selectedResult = result))
    }

    fun searchLoading(
        state: CurrentTrackUiState,
        append: Boolean,
    ): CurrentTrackUiState {
        return state.copy(
            search = state.search.copy(
                isSearching = !append,
                isLoadingNextPage = append,
                errorMessage = null,
            )
        )
    }

    fun searchLoaded(
        state: CurrentTrackUiState,
        page: MusicSearchPage,
        append: Boolean,
    ): CurrentTrackUiState {
        val oldResults = if (append) state.search.results else emptyList()
        return state.copy(
            search = state.search.copy(
                results = oldResults + page.results,
                nextPageToken = page.nextPageToken,
                isSearching = false,
                isLoadingNextPage = false,
                errorMessage = null,
            )
        )
    }

    fun searchFailed(
        state: CurrentTrackUiState,
        error: Throwable,
    ): CurrentTrackUiState {
        return state.copy(
            search = state.search.copy(
                isSearching = false,
                isLoadingNextPage = false,
                errorMessage = error.message,
            )
        )
    }
}
