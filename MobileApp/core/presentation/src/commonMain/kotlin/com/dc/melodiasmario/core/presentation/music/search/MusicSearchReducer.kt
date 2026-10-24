package com.dc.melodiasmario.core.presentation.music.search

import com.dc.melodiasmario.core.presentation.music.model.MAddMusicMode
import com.dc.melodiasmario.core.presentation.music.model.MMusicSearchKind
import com.dc.melodiasmario.core.presentation.music.state.MAddMusicSheetState
import com.dc.melodiasmario.core.model.search.MusicSearchCapability
import com.dc.melodiasmario.core.model.search.MusicSearchPage
import com.dc.melodiasmario.core.presentation.music.toProviderUi
import com.dc.melodiasmario.core.presentation.music.toResultUi

class MusicSearchReducer {

    fun reset(commandInFlight: Boolean): MAddMusicSheetState {
        return MAddMusicSheetState(commandInFlight = commandInFlight)
    }

    fun queryChanged(state: MAddMusicSheetState, query: String): MAddMusicSheetState {
        return clearResults(state).copy(query = query)
    }

    fun providerChanged(state: MAddMusicSheetState, providerId: String): MAddMusicSheetState {
        return clearResults(state).copy(selectedProviderId = providerId)
    }

    fun kindChanged(state: MAddMusicSheetState, kind: MMusicSearchKind): MAddMusicSheetState {
        return clearResults(state).copy(selectedKind = kind)
    }

    fun modeChanged(state: MAddMusicSheetState, mode: MAddMusicMode): MAddMusicSheetState {
        return state.copy(
            mode = mode,
            selectedResultId = null,
            errorMessage = null
        )
    }

    fun resultSelected(state: MAddMusicSheetState, resultId: String): MAddMusicSheetState {
        return state.copy(selectedResultId = resultId)
    }

    fun capabilitiesLoading(state: MAddMusicSheetState): MAddMusicSheetState {
        return state.copy(
            isCapabilitiesLoading = true,
            errorMessage = null
        )
    }

    fun capabilitiesLoaded(state: MAddMusicSheetState, capabilities: List<MusicSearchCapability>): MAddMusicSheetState {
        val providers = capabilities.mapNotNull { it.toProviderUi() }

        val selectedProviderId =
            state.selectedProviderId.takeIf { selectedId ->
                providers.any { it.id == selectedId }
            } ?: providers.firstOrNull {
                it.canEnqueue
            }?.id.orEmpty()

        return state.copy(
            providers = providers,
            selectedProviderId = selectedProviderId,
            isCapabilitiesLoading = false,
            errorMessage = null
        )
    }

    fun capabilitiesFailed(state: MAddMusicSheetState, error: Throwable): MAddMusicSheetState {
        return state.copy(
            isCapabilitiesLoading = false,
            errorMessage = error.message
        )
    }

    fun searchLoading(state: MAddMusicSheetState,append: Boolean): MAddMusicSheetState {
        return state.copy(
            isSearching = !append,
            isLoadingNextPage = append,
            selectedResultId = if (append) state.selectedResultId else null,
            errorMessage = null
        )
    }

    fun searchLoaded(state: MAddMusicSheetState, page: MusicSearchPage, append: Boolean): MAddMusicSheetState {
        val previousResults = if (append) state.results else emptyList()

        val results = (previousResults + page.results.map { it.toResultUi() }).distinctBy { it.id }

        return state.copy(
            results = results,
            nextPageToken = page.nextPageToken,
            isSearching = false,
            isLoadingNextPage = false,
            errorMessage = null
        )
    }

    fun searchFailed(state: MAddMusicSheetState, error: Throwable): MAddMusicSheetState {
        return state.copy(
            isSearching = false,
            isLoadingNextPage = false,
            errorMessage = error.message
        )
    }

    private fun clearResults(state: MAddMusicSheetState): MAddMusicSheetState {
        return state.copy(
            results = emptyList(),
            selectedResultId = null,
            nextPageToken = null,
            isSearching = false,
            isLoadingNextPage = false,
            errorMessage = null
        )
    }
}