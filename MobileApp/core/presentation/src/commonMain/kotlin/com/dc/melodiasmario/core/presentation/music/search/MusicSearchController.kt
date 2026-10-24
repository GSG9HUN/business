package com.dc.melodiasmario.core.presentation.music.search

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.common.TrackSearchPrefix
import com.dc.melodiasmario.core.presentation.MviEffect
import com.dc.melodiasmario.core.presentation.runAction
import com.dc.melodiasmario.core.presentation.music.model.MAddMusicMode
import com.dc.melodiasmario.core.presentation.music.model.MMusicSearchKind
import com.dc.melodiasmario.core.presentation.music.state.MAddMusicSheetState
import com.dc.melodiasmario.core.presentation.music.toDomainKind
import com.dc.melodiasmario.core.presentation.music.toTrackSearchPrefixOrDefault
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Job
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.filter
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import kotlin.time.Duration.Companion.milliseconds
import com.dc.melodiasmario.core.model.search.MusicSearchCapability
import com.dc.melodiasmario.core.model.search.MusicSearchPage
import com.dc.melodiasmario.core.model.search.MusicSearchResultKind

class MusicSearchController<State, Effect>(
    private val getCapabilities: (String) -> Flow<Resource<List<MusicSearchCapability>>>,
    private val searchMusic: (String, TrackSearchPrefix, MusicSearchResultKind, String, String?) -> Flow<Resource<MusicSearchPage>>,
    private val state: MutableStateFlow<State>,
    private val effects: MutableSharedFlow<Effect>,
    private val failureEffect: Effect,
    private val scope: CoroutineScope,
    private val currentGuildIdProvider: () -> String?
) where State : MusicSearchUiState<State>, Effect : MviEffect {

    private var searchJob: Job? = null
    private val musicSearchReducer = MusicSearchReducer()
    private var searchRequestKey = 0L

    fun resetForGuildChange(newGuildId: String, currentGuildId: String?) {
        if (newGuildId == currentGuildId) {
            return
        }

        clear()
    }

    suspend fun loadCapabilities() {
        val guildId = currentGuildIdProvider() ?: return

        getCapabilities(guildId).filter {
            currentGuildIdProvider() == guildId
        }.runAction(
            state = state,
            effects = effects,
            failureEffect = failureEffect,
            onLoading = {
                it.withAddMusic(
                    musicSearchReducer.capabilitiesLoading(it.addMusic)
                )
            },
            onSuccessWithData = { currentState, capabilities ->
                currentState.withAddMusic(
                    musicSearchReducer.capabilitiesLoaded(currentState.addMusic, capabilities)
                )
            },
            onError = { currentState, error ->
                currentState.withAddMusic(
                    musicSearchReducer.capabilitiesFailed(currentState.addMusic, error)
                )
            },
        )
    }

    fun onQueryChanged(query: String) {
        cancelInFlightSearch()

        state.update {
            it.withAddMusic(
                musicSearchReducer.queryChanged(it.addMusic, query)
            )
        }

        if (query.trim().length < MIN_SEARCH_QUERY_LENGTH) {
            return
        }

        startSearch(append = false, debounce = true)
    }

    fun onProviderChanged(providerId: String) {
        cancelInFlightSearch()

        state.update {
            it.withAddMusic(
                musicSearchReducer.providerChanged(it.addMusic, providerId)
            )
        }

        submit()
    }

    fun onKindChanged(kind: MMusicSearchKind) {
        cancelInFlightSearch()

        state.update {
            it.withAddMusic(musicSearchReducer.kindChanged(it.addMusic, kind))
        }

        submit()
    }

    fun submit() {
        startSearch(append = false, debounce = false)
    }

    fun retry() {
        submit()
    }

    fun loadNextPage() {
        val search = state.value.addMusic

        if (search.nextPageToken == null || search.isSearching || search.isLoadingNextPage) {
            return
        }

        startSearch(append = true, debounce = false)
    }

    fun selectResult(resultId: String) {
        state.update {
            it.withAddMusic(
                musicSearchReducer.resultSelected(it.addMusic, resultId)
            )
        }
    }

    fun updateMode(mode: MAddMusicMode) {
        state.update {
            it.withAddMusic(
                musicSearchReducer.modeChanged(it.addMusic, mode)
            )
        }
    }

    fun clear() {
        cancel()

        state.update {
            it.withAddMusic(
                musicSearchReducer.reset(commandInFlight = it.isCommandInFlight)
            )
        }
    }

    fun cancel() {
        cancelInFlightSearch()
    }

    private fun startSearch(append: Boolean, debounce: Boolean) {
        val guildId = currentGuildIdProvider() ?: return
        val search = state.value.addMusic

        if (!canSearch(search)) {
            return
        }

        if (append && search.nextPageToken == null) {
            return
        }

        cancelInFlightSearch()
        val requestKey = searchRequestKey

        state.update {
            it.withAddMusic(musicSearchReducer.searchLoading(it.addMusic, append))
        }

        searchJob = scope.launch {
            if (debounce) {
                delay(SEARCH_DEBOUNCE_MILLIS.milliseconds)
            }

            performSearch(
                guildId = guildId,
                requestKey = requestKey,
                search = search,
                append = append,
            )
        }
    }

    private fun canSearch(search: MAddMusicSheetState): Boolean {
        if (search.isCapabilitiesLoading || search.query.trim().length < MIN_SEARCH_QUERY_LENGTH) {
            return false
        }

        val provider = search.providers.firstOrNull {
            it.id == search.selectedProviderId
        } ?: return false

        if (!provider.canEnqueue) {
            return false
        }

        return when (search.selectedKind) {
            MMusicSearchKind.Track -> provider.supportsTrackSearch
            MMusicSearchKind.Playlist -> provider.supportsPlaylistSearch
        }
    }

    private suspend fun performSearch(
        guildId: String,
        requestKey: Long,
        search: MAddMusicSheetState,
        append: Boolean
    ) {
        searchMusic(
            guildId,
            search.selectedProviderId.toTrackSearchPrefixOrDefault(),
            search.selectedKind.toDomainKind(),
            search.query.trim(),
            if (append) search.nextPageToken else null,
        ).filter {
            requestKey == searchRequestKey && currentGuildIdProvider() == guildId
        }.runAction(
            state = state,
            effects = effects,
            failureEffect = failureEffect,
            onLoading = {
                it.withAddMusic(musicSearchReducer.searchLoading(it.addMusic, append))
            },
            onSuccessWithData = { currentState, page ->
                currentState.withAddMusic(
                    musicSearchReducer.searchLoaded(currentState.addMusic, page, append)
                )
            },
            onError = { currentState, error ->
                currentState.withAddMusic(
                    musicSearchReducer.searchFailed(currentState.addMusic, error)
                )
            },
        )
    }

    private fun cancelInFlightSearch() {
        searchRequestKey += 1

        searchJob?.cancel()
        searchJob = null
    }

    private companion object {
        const val MIN_SEARCH_QUERY_LENGTH = 2
        const val SEARCH_DEBOUNCE_MILLIS = 350L
    }
}