package com.dc.melodiasmario.feature.currenttrack.presentation.search

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.common.presentation.runAction
import com.dc.melodiasmario.core.commonui.music.model.MAddMusicMode
import com.dc.melodiasmario.core.commonui.music.model.MMusicSearchKind
import com.dc.melodiasmario.core.domain.search.usecase.GetMusicSearchCapabilitiesUseCase
import com.dc.melodiasmario.core.domain.search.usecase.SearchMusicUseCase
import com.dc.melodiasmario.feature.currenttrack.presentation.CurrentTrackEffect
import com.dc.melodiasmario.feature.currenttrack.presentation.state.CurrentTrackUiState
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Job
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch

internal class CurrentTrackSearchController(
    private val getMusicSearchCapabilitiesUseCase: GetMusicSearchCapabilitiesUseCase,
    private val searchMusicUseCase: SearchMusicUseCase,
    private val state: MutableStateFlow<CurrentTrackUiState>,
    private val effects: MutableSharedFlow<CurrentTrackEffect>,
    private val scope: CoroutineScope,
    private val currentGuildIdProvider: () -> String?,
) {
    private var searchJob: Job? = null
    private var searchRequestKey = 0

    fun resetForGuildChange(newGuildId: String, currentGuildId: String?) {
        if (currentGuildId == newGuildId) return

        cancelInFlightSearch()
        state.update(CurrentTrackSearchReducer::reset)
    }

    suspend fun loadCapabilities() {
        val guildId = currentGuildIdProvider() ?: return

        getMusicSearchCapabilitiesUseCase(guildId).runAction(
            state = state,
            effects = effects,
            failureEffect = CurrentTrackEffect.SearchFailed,
            onLoading = CurrentTrackSearchReducer::capabilitiesLoading,
            onSuccessWithData = CurrentTrackSearchReducer::capabilitiesLoaded,
            onError = CurrentTrackSearchReducer::capabilitiesFailed,
        )
    }

    fun onQueryChanged(query: String) {
        state.update { CurrentTrackSearchReducer.queryChanged(it, query) }
        searchJob?.cancel()

        if (query.trim().length < MinSearchQueryLength) {
            state.update(CurrentTrackSearchReducer::clearShortQueryResults)
            return
        }

        val requestKey = nextRequestKey()
        searchJob = scope.launch {
            delay(SearchDebounceMillis)
            performSearch(requestKey, append = false)
        }
    }

    fun onProviderChanged(providerId: String) {
        cancelInFlightSearch()
        state.update { CurrentTrackSearchReducer.providerChanged(it, providerId) }

        if (state.value.addMusic.query.trim().length >= MinSearchQueryLength) {
            submit()
        }
    }

    fun onKindChanged(kind: MMusicSearchKind) {
        cancelInFlightSearch()
        state.update { CurrentTrackSearchReducer.kindChanged(it, kind) }

        if (state.value.addMusic.query.trim().length >= MinSearchQueryLength) {
            submit()
        }
    }

    fun submit() {
        searchJob?.cancel()
        val requestKey = nextRequestKey()
        searchJob = scope.launch {
            performSearch(requestKey, append = false)
        }
    }

    fun retry() {
        submit()
    }

    fun loadNextPage() {
        val search = state.value.addMusic
        if (search.nextPageToken == null || search.isLoadingNextPage || search.isSearching) return

        val requestKey = nextRequestKey()
        searchJob = scope.launch {
            performSearch(requestKey, append = true)
        }
    }

    fun selectResult(resultId: String) {
        state.update { CurrentTrackSearchReducer.resultSelected(it, resultId = resultId) }
    }

    fun updateMode(mode: MAddMusicMode) {
        state.update { CurrentTrackSearchReducer.modeChanged(it, mode) }
    }

    fun clear() {
        cancelInFlightSearch()
        state.update(CurrentTrackSearchReducer::reset)
    }

    fun cancel() {
        searchJob?.cancel()
        searchJob = null
    }

    private suspend fun performSearch(
        requestKey: Int,
        append: Boolean,
    ) {
        val guildId = currentGuildIdProvider() ?: return
        val search = state.value.addMusic
        val query = search.query.trim()

        val provider = search.selectedProviderId.toTrackSearchPrefixOrDefault()
        val kind = search.selectedKind.toDomainKind()
        if (query.length < MinSearchQueryLength) return

        searchMusicUseCase(
            guildId = guildId,
            provider = provider,
            kind = kind,
            query = query,
            pageToken = if (append) search.nextPageToken else null,
        ).collect { result ->
            if (requestKey != searchRequestKey) return@collect

            when (result) {
                Resource.Loading -> {
                    state.update { CurrentTrackSearchReducer.searchLoading(it, append) }
                }

                is Resource.Success -> {
                    state.update {
                        CurrentTrackSearchReducer.searchLoaded(
                            state = it,
                            page = result.data,
                            append = append,
                        )
                    }
                }

                is Resource.Error -> {
                    state.update {
                        CurrentTrackSearchReducer.searchFailed(
                            state = it,
                            error = result.error,
                        )
                    }
                }
            }
        }
    }

    private fun cancelInFlightSearch() {
        searchJob?.cancel()
        searchJob = null
        nextRequestKey()
    }

    private fun nextRequestKey(): Int {
        searchRequestKey += 1
        return searchRequestKey
    }

    private companion object {
        const val MinSearchQueryLength = 2
        const val SearchDebounceMillis = 350L
    }
}
