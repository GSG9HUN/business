package com.dc.melodiasmario.core.commonui.music.state

import com.dc.melodiasmario.core.commonui.music.model.MAddMusicMode
import com.dc.melodiasmario.core.commonui.music.model.MMusicSearchKind
import com.dc.melodiasmario.core.commonui.music.model.MMusicSearchProviderUi
import com.dc.melodiasmario.core.commonui.music.model.MMusicSearchResultUi

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

    val commandInFlight: Boolean = false,
)