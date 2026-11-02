package com.dc.melodiasmario.core.presentation.music.search

import com.dc.melodiasmario.core.model.search.MusicSearchCapability
import com.dc.melodiasmario.core.model.search.MusicSearchPage
import com.dc.melodiasmario.core.presentation.music.model.MAddMusicMode
import com.dc.melodiasmario.core.presentation.music.model.MMusicSearchKind
import com.dc.melodiasmario.core.presentation.music.state.MAddMusicSheetState
import com.dc.melodiasmario.core.presentation.music.toProviderUi
import com.dc.melodiasmario.core.presentation.music.toResultUi

/**
 * Creates updated music search presentation state from the supplied changes.
 */
class MusicSearchReducer {

    /**
     * Resets music search reducer to its initial state.
     */
    fun reset(commandInFlight: Boolean): MAddMusicSheetState {
        return MAddMusicSheetState(commandInFlight = commandInFlight)
    }

    /**
     * Provides the query changed operation for music search reducer.
     */
    fun queryChanged(mAddMusicSheetState: MAddMusicSheetState, query: String): MAddMusicSheetState {
        return clearResults(mAddMusicSheetState = mAddMusicSheetState).copy(query = query)
    }

    /**
     * Provides the provider changed operation for music search reducer.
     */
    fun providerChanged(mAddMusicSheetState: MAddMusicSheetState, providerId: String): MAddMusicSheetState {
        return clearResults(mAddMusicSheetState = mAddMusicSheetState).copy(selectedProviderId = providerId)
    }

    /**
     * Provides the kind changed operation for music search reducer.
     */
    fun kindChanged(mAddMusicSheetState: MAddMusicSheetState, kind: MMusicSearchKind): MAddMusicSheetState {
        return clearResults(mAddMusicSheetState = mAddMusicSheetState).copy(selectedKind = kind)
    }

    /**
     * Provides the mode changed operation for music search reducer.
     */
    fun modeChanged(mAddMusicSheetState: MAddMusicSheetState, mode: MAddMusicMode): MAddMusicSheetState {
        return mAddMusicSheetState.copy(
            mode = mode,
            selectedResultId = null,
            errorMessage = null
        )
    }

    /**
     * Provides the result selected operation for music search reducer.
     */
    fun resultSelected(mAddMusicSheetState: MAddMusicSheetState, resultId: String): MAddMusicSheetState {
        return mAddMusicSheetState.copy(selectedResultId = resultId)
    }

    /**
     * Provides the capabilities loading operation for music search reducer.
     */
    fun capabilitiesLoading(mAddMusicSheetState: MAddMusicSheetState): MAddMusicSheetState {
        return mAddMusicSheetState.copy(
            isCapabilitiesLoading = true,
            errorMessage = null
        )
    }

    /**
     * Provides the capabilities loaded operation for music search reducer.
     */
    fun capabilitiesLoaded(mAddMusicSheetState: MAddMusicSheetState, capabilities: List<MusicSearchCapability>): MAddMusicSheetState {
        val providers = capabilities.mapNotNull { searchCapability -> searchCapability.toProviderUi() }

        val selectedProviderId =
            mAddMusicSheetState.selectedProviderId.takeIf { selectedId ->
                providers.any { provider -> provider.id == selectedId }
            } ?: providers.firstOrNull { provider ->
                provider.canEnqueue
            }?.id
                .orEmpty()

        return mAddMusicSheetState.copy(
            providers = providers,
            selectedProviderId = selectedProviderId,
            isCapabilitiesLoading = false,
            errorMessage = null
        )
    }

    /**
     * Provides the capabilities failed operation for music search reducer.
     */
    fun capabilitiesFailed(mAddMusicSheetState: MAddMusicSheetState, error: Throwable): MAddMusicSheetState {
        return mAddMusicSheetState.copy(
            isCapabilitiesLoading = false,
            errorMessage = error.message
        )
    }

    /**
     * Provides the search loading operation for music search reducer.
     */
    fun searchLoading(mAddMusicSheetState: MAddMusicSheetState,append: Boolean): MAddMusicSheetState {
        return mAddMusicSheetState.copy(
            isSearching = !append,
            isLoadingNextPage = append,
            selectedResultId = if (append) mAddMusicSheetState.selectedResultId else null,
            errorMessage = null
        )
    }

    /**
     * Provides the search loaded operation for music search reducer.
     */
    fun searchLoaded(mAddMusicSheetState: MAddMusicSheetState, page: MusicSearchPage, append: Boolean): MAddMusicSheetState {
        val previousResults = if (append) mAddMusicSheetState.results else emptyList()

        val results =
            (previousResults + page.results.map { searchResult -> searchResult.toResultUi() }).distinctBy { searchResult -> searchResult.id }

        return mAddMusicSheetState.copy(
            results = results,
            nextPageToken = page.nextPageToken,
            isSearching = false,
            isLoadingNextPage = false,
            errorMessage = null
        )
    }

    /**
     * Provides the search failed operation for music search reducer.
     */
    fun searchFailed(mAddMusicSheetState: MAddMusicSheetState, error: Throwable): MAddMusicSheetState {
        return mAddMusicSheetState.copy(
            isSearching = false,
            isLoadingNextPage = false,
            errorMessage = error.message
        )
    }

    private fun clearResults(mAddMusicSheetState: MAddMusicSheetState): MAddMusicSheetState {
        return mAddMusicSheetState.copy(
            results = emptyList(),
            selectedResultId = null,
            nextPageToken = null,
            isSearching = false,
            isLoadingNextPage = false,
            errorMessage = null
        )
    }
}
