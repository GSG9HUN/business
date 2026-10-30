package com.dc.melodiasmario.core.presentation.music.state

import com.dc.melodiasmario.core.presentation.music.model.MAddMusicMode
import com.dc.melodiasmario.core.presentation.music.model.MMusicSearchKind
import com.dc.melodiasmario.core.presentation.music.model.MMusicSearchProviderUi
import com.dc.melodiasmario.core.presentation.music.model.MMusicSearchResultUi

/**
 * Provides add music sheet state for the state layer.
 *
 * @property mode Mode.
 * @property query Query.
 * @property selectedProviderId Selected provider id.
 * @property providers Providers.
 * @property selectedKind Selected kind.
 * @property results Results.
 * @property selectedResultId Selected result id.
 * @property nextPageToken Next page token.
 * @property isCapabilitiesLoading Is capabilities loading.
 * @property isSearching Is searching.
 * @property isLoadingNextPage Is loading next page.
 * @property errorMessage Error message.
 * @property manualDraft Manual draft.
 * @property canSubmitManual Can submit manual.
 * @property isManualLoading Is manual loading.
 * @property commandInFlight Command in flight.
 */
data class MAddMusicSheetState(
    val mode: MAddMusicMode = MAddMusicMode.Search,

    val query: String = "",
    val selectedProviderId: String = "",
    val providers: List<MMusicSearchProviderUi> = emptyList(),

    val selectedKind: MMusicSearchKind = MMusicSearchKind.Track,
    val results: List<MMusicSearchResultUi> = emptyList(),
    val selectedResultId: String? = null,
    val nextPageToken: String? = null,

    val isCapabilitiesLoading: Boolean = false,
    val isSearching: Boolean = false,
    val isLoadingNextPage: Boolean = false,
    val errorMessage: String? = null,

    val manualDraft: String = "",
    val canSubmitManual: Boolean = false,
    val isManualLoading: Boolean = false,

    val commandInFlight: Boolean = false
)
