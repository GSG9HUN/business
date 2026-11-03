package com.dc.melodiasmario.core.presentation.music.search

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.common.TrackSearchPrefix
import com.dc.melodiasmario.core.model.search.MusicSearchCapability
import com.dc.melodiasmario.core.model.search.MusicSearchPage
import com.dc.melodiasmario.core.model.search.MusicSearchResultKind
import com.dc.melodiasmario.core.presentation.MviEffect
import com.dc.melodiasmario.core.presentation.music.model.MAddMusicMode
import com.dc.melodiasmario.core.presentation.music.model.MMusicSearchKind
import com.dc.melodiasmario.core.presentation.music.state.MAddMusicSheetState
import com.dc.melodiasmario.core.presentation.music.toDomainKind
import com.dc.melodiasmario.core.presentation.music.toTrackSearchPrefixOrDefault
import com.dc.melodiasmario.core.presentation.runAction
import kotlin.time.Duration.Companion.milliseconds
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Job
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.filter
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch

/**
 * Owns music search coordination and its lifecycle-bound operations.
 */
class MusicSearchController<State, Effect>(
    private val getCapabilities: (guildId: String) -> Flow<Resource<List<MusicSearchCapability>>>,
    private val searchMusic: (
        guildId: String,
        provider: TrackSearchPrefix,
        kind: MusicSearchResultKind,
        query: String,
        continuationToken: String?
    ) -> Flow<Resource<MusicSearchPage>>,
    private val uiState: MutableStateFlow<State>,
    private val effects: MutableSharedFlow<Effect>,
    private val failureEffect: Effect,
    private val coroutineScope: CoroutineScope,
    private val currentGuildIdProvider: () -> String?
) where State : MusicSearchUiState<State>, Effect : MviEffect {

    private var searchJob: Job? = null
    private val musicSearchReducer = MusicSearchReducer()
    private var searchRequestKey = 0L

    /**
     * Clears search state when the selected guild changes.
     */
    fun resetForGuildChange(newGuildId: String, currentGuildId: String?): Unit {
        if (newGuildId == currentGuildId) {
            return
        }

        clear()
    }

    /**
     * Loads music-search capabilities for the current guild.
     */
    suspend fun loadCapabilities(): Unit {
        val guildId = currentGuildIdProvider() ?: return

        getCapabilities(guildId).filter {
            currentGuildIdProvider() == guildId
        }
            .runAction(
                uiState = uiState,
                effects = effects,
                failureEffect = failureEffect,
                onLoading = { currentUiState ->
                    currentUiState.withAddMusic(
                        addMusic = musicSearchReducer.capabilitiesLoading(mAddMusicSheetState = currentUiState.addMusic)
                    )
                },
                onSuccessWithData = { currentState, capabilities ->
                    currentState.withAddMusic(
                        addMusic = musicSearchReducer.capabilitiesLoaded(
                            mAddMusicSheetState = currentState.addMusic,
                            capabilities = capabilities
                        )
                    )
                },
                onError = { currentState, error ->
                    currentState.withAddMusic(
                        addMusic = musicSearchReducer.capabilitiesFailed(
                            mAddMusicSheetState = currentState.addMusic,
                            error = error
                        )
                    )
                }
            )
    }

    /**
     * Updates the search query and invalidates obsolete results.
     */
    fun onQueryChanged(query: String): Unit {
        cancelInFlightSearch()

        uiState.update { currentUiState ->
            currentUiState.withAddMusic(
                addMusic = musicSearchReducer.queryChanged(mAddMusicSheetState = currentUiState.addMusic, query = query)
            )
        }

        if (query.trim().length < MIN_SEARCH_QUERY_LENGTH) {
            return
        }

        startSearch(append = false, debounce = true)
    }

    /**
     * Updates the selected search provider.
     */
    fun onProviderChanged(providerId: String): Unit {
        cancelInFlightSearch()

        uiState.update { currentUiState ->
            currentUiState.withAddMusic(
                addMusic = musicSearchReducer.providerChanged(mAddMusicSheetState = currentUiState.addMusic, providerId = providerId)
            )
        }

        submit()
    }

    /**
     * Updates the selected music-search result kind.
     */
    fun onKindChanged(kind: MMusicSearchKind): Unit {
        cancelInFlightSearch()

        uiState.update { currentUiState ->
            currentUiState.withAddMusic(addMusic = musicSearchReducer.kindChanged(mAddMusicSheetState = currentUiState.addMusic, kind = kind))
        }

        submit()
    }

    /**
     * Submits the current search query through the configured search operation.
     */
    fun submit(): Unit {
        startSearch(append = false, debounce = false)
    }

    /**
     * Retries the current music search.
     */
    fun retry(): Unit {
        submit()
    }

    /**
     * Loads the next page of search results when available.
     */
    fun loadNextPage(): Unit {
        val search = uiState.value.addMusic

        if (search.nextPageToken == null
            || search.isSearching
            || search.isLoadingNextPage) {
            return
        }

        startSearch(append = true, debounce = false)
    }

    /**
     * Updates the selected search-result identifier.
     */
    fun selectResult(resultId: String): Unit {
        uiState.update { currentUiState ->
            currentUiState.withAddMusic(
                addMusic = musicSearchReducer.resultSelected(mAddMusicSheetState = currentUiState.addMusic, resultId = resultId)
            )
        }
    }

    /**
     * Updates the music-input mode.
     */
    fun updateMode(mode: MAddMusicMode): Unit {
        uiState.update { currentUiState ->
            currentUiState.withAddMusic(
                addMusic = musicSearchReducer.modeChanged(mAddMusicSheetState = currentUiState.addMusic, mode = mode)
            )
        }
    }

    /**
     * Clears the state and resources owned by MusicSearchController.
     */
    fun clear(): Unit {
        cancel()

        uiState.update { currentUiState ->
            currentUiState.withAddMusic(
                addMusic = musicSearchReducer.reset(commandInFlight = currentUiState.isCommandInFlight)
            )
        }
    }

    /**
     * Cancels the active music search controller.
     */
    fun cancel(): Unit {
        cancelInFlightSearch()
    }

    private fun startSearch(append: Boolean, debounce: Boolean) {
        val guildId = currentGuildIdProvider() ?: return
        val search = uiState.value.addMusic

        if (!canSearch(search = search)) {
            return
        }

        if (append && search.nextPageToken == null) {
            return
        }

        cancelInFlightSearch()
        val requestKey = searchRequestKey

        uiState.update { currentUiState ->
            currentUiState.withAddMusic(
                addMusic = musicSearchReducer.searchLoading(
                    mAddMusicSheetState = currentUiState.addMusic,
                    append = append
                )
            )
        }

        searchJob = coroutineScope.launch {
            if (debounce) {
                delay(duration = SEARCH_DEBOUNCE_MILLIS.milliseconds)
            }

            performSearch(
                guildId = guildId,
                requestKey = requestKey,
                search = search,
                append = append
            )
        }
    }

    private fun canSearch(search: MAddMusicSheetState): Boolean {
        if (search.isCapabilitiesLoading
            || search.query.trim().length < MIN_SEARCH_QUERY_LENGTH) {
            return false
        }

        val provider = search.providers.firstOrNull { provider ->
            provider.id == search.selectedProviderId
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
            if (append) {
                search.nextPageToken
            } else {
                null
            }
        ).filter {
            requestKey == searchRequestKey && currentGuildIdProvider() == guildId
        }
            .runAction(
                uiState = uiState,
                effects = effects,
                failureEffect = failureEffect,
                onLoading = { currentUiState ->
                    currentUiState.withAddMusic(
                        addMusic = musicSearchReducer.searchLoading(
                            mAddMusicSheetState = currentUiState.addMusic,
                            append = append
                        )
                    )
                },
                onSuccessWithData = { currentState, page ->
                    currentState.withAddMusic(
                        addMusic = musicSearchReducer.searchLoaded(
                            mAddMusicSheetState = currentState.addMusic,
                            page = page,
                            append = append
                        )
                    )
                },
                onError = { currentState, error ->
                    currentState.withAddMusic(
                        addMusic = musicSearchReducer.searchFailed(
                            mAddMusicSheetState = currentState.addMusic,
                            error = error
                        )
                    )
                }
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
