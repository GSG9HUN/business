package com.dc.melodiasmario.feature.currenttrack.presentation.search

import com.dc.melodiasmario.core.commonui.music.model.MAddMusicMode
import com.dc.melodiasmario.core.commonui.music.model.MMusicSearchKind
import com.dc.melodiasmario.core.commonui.music.state.MAddMusicSheetState
import com.dc.melodiasmario.core.model.search.MusicSearchCapability
import com.dc.melodiasmario.core.model.search.MusicSearchPage
import com.dc.melodiasmario.feature.currenttrack.presentation.state.CurrentTrackUiState

internal object CurrentTrackSearchReducer {
    fun reset(state: CurrentTrackUiState): CurrentTrackUiState {
        return state.copy(
            addMusic = MAddMusicSheetState(
                commandInFlight = state.actions.isCommandInFlight,
            )
        )
    }

    fun queryChanged(state: CurrentTrackUiState, query: String): CurrentTrackUiState {
        return state.copy(
            addMusic = state.addMusic.copy(
                query = query,
                selectedResultId = null,
            )
        )
    }

    fun clearShortQueryResults(state: CurrentTrackUiState): CurrentTrackUiState {
        return state.copy(
            addMusic = state.addMusic.copy(
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
            addMusic = state.addMusic.copy(
                isCapabilitiesLoading = true,
                errorMessage = null,
            )
        )
    }

    fun capabilitiesLoaded(
        state: CurrentTrackUiState,
        capabilities: List<MusicSearchCapability>,
    ): CurrentTrackUiState {
        val providerStates = capabilities.mapNotNull { it.toProviderUi() }

        return state.copy(
            addMusic = state.addMusic.copy(
                providers = providerStates,
                selectedProviderId = state.addMusic.selectedProviderId.ifBlank {
                    providerStates.firstOrNull { it.canEnqueue }?.id.orEmpty()
                },
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
            addMusic = state.addMusic.copy(
                isCapabilitiesLoading = false,
                errorMessage = error.message,
            )
        )
    }

    fun providerChanged(
        state: CurrentTrackUiState,
        providerId: String,
    ): CurrentTrackUiState {
        return state.copy(
            addMusic = state.addMusic.copy(
                selectedProviderId = providerId,
                results = emptyList(),
                selectedResultId = null,
                nextPageToken = null,
                errorMessage = null,
                isSearching = false,
                isLoadingNextPage = false,
            )
        )
    }

    fun kindChanged(
        state: CurrentTrackUiState,
        kind: MMusicSearchKind,
    ): CurrentTrackUiState {
        return state.copy(
            addMusic = state.addMusic.copy(
                selectedKind = kind,
                results = emptyList(),
                selectedResultId = null,
                nextPageToken = null,
                errorMessage = null,
                isSearching = false,
                isLoadingNextPage = false,
            )
        )
    }

    fun modeChanged(
        state: CurrentTrackUiState,
        mode: MAddMusicMode,
    ): CurrentTrackUiState {
        return state.copy(
            addMusic = state.addMusic.copy(
                mode = mode,
                selectedResultId = null,
                errorMessage = null,
            )
        )
    }

    fun resultSelected(
        state: CurrentTrackUiState,
        resultId: String,
    ): CurrentTrackUiState {
        return state.copy(
            addMusic = state.addMusic.copy(selectedResultId = resultId)
        )
    }

    fun searchLoading(
        state: CurrentTrackUiState,
        append: Boolean,
    ): CurrentTrackUiState {
        return state.copy(
            addMusic = state.addMusic.copy(
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
        val oldResults = if (append) state.addMusic.results else emptyList()

        return state.copy(
            addMusic = state.addMusic.copy(
                results = oldResults + page.results.map { it.toResultUi() },
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
            addMusic = state.addMusic.copy(
                isSearching = false,
                isLoadingNextPage = false,
                errorMessage = error.message,
            )
        )
    }
}
