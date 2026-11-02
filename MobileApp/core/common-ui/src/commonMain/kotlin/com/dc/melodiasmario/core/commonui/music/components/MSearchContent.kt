package com.dc.melodiasmario.core.commonui.music.components

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.material3.Button
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalFocusManager
import androidx.compose.ui.platform.LocalSoftwareKeyboardController
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.components.input.MSearchBar
import com.dc.melodiasmario.core.commonui.music.model.MAddMusicLabels
import com.dc.melodiasmario.core.presentation.music.model.MMusicSearchKind
import com.dc.melodiasmario.core.presentation.music.state.MAddMusicSheetState

/**
 * Renders search content from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun MSearchContent(
    mAddMusicSheetState: MAddMusicSheetState,
    mAddMusicLabels: MAddMusicLabels,
    onQueryChanged: (query: String) -> Unit,
    onSearchSubmitted: () -> Unit,
    onProviderChanged: (providerId: String) -> Unit,
    onKindChanged: (musicSearchKind: MMusicSearchKind) -> Unit,
    onResultClicked: (resultId: String) -> Unit,
    onSelectedResultAddClicked: () -> Unit,
    onNextPageRequested: () -> Unit,
    onSearchRetryClicked: () -> Unit,
    onCapabilitiesRetryClicked: () -> Unit
): Unit {
    val focusManager = LocalFocusManager.current
    val keyboardController = LocalSoftwareKeyboardController.current
    val selectedProvider =
        mAddMusicSheetState.providers.firstOrNull { provider -> provider.id == mAddMusicSheetState.selectedProviderId }
    val providerUnavailable = mAddMusicSheetState.providers.isNotEmpty() &&
            (selectedProvider == null || !selectedProvider.canEnqueue)
    val kindUnavailable = selectedProvider != null && when (mAddMusicSheetState.selectedKind) {
        MMusicSearchKind.Track -> !selectedProvider.supportsTrackSearch
        MMusicSearchKind.Playlist -> !selectedProvider.supportsPlaylistSearch
    }
    val canSearch = !mAddMusicSheetState.isCapabilitiesLoading && !providerUnavailable && !kindUnavailable
    val selectedResult =
        mAddMusicSheetState.results.firstOrNull { searchResult -> searchResult.id == mAddMusicSheetState.selectedResultId }
    fun submitSearch() {
        keyboardController?.hide()
        focusManager.clearFocus()
        onSearchSubmitted()
    }

    Column(
        modifier = Modifier.fillMaxWidth(),
        verticalArrangement = Arrangement.spacedBy(space = 12.dp)
    ) {
        MProviderSelector(
            label = mAddMusicLabels.providerLabel,
            selectedProvider = selectedProvider,
            providers = mAddMusicSheetState.providers,
            onProviderChanged = onProviderChanged
        )

        MKindSelector(
            mAddMusicLabels = mAddMusicLabels,
            selectedKind = mAddMusicSheetState.selectedKind,
            onKindChanged = onKindChanged
        )

        Row(
            modifier = Modifier.fillMaxWidth(),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.spacedBy(space = 8.dp)
        ) {
            MSearchBar(
                modifier = Modifier.weight(weight = 1f),
                query = mAddMusicSheetState.query,
                onQueryChange = onQueryChanged,
                placeholder = mAddMusicLabels.searchPlaceholder,
                enabled = canSearch,
                onSearch = ::submitSearch
            )
            Button(
                onClick = ::submitSearch,
                enabled = canSearch && mAddMusicSheetState.query.trim().length >= 2
            ) {
                MText(text = mAddMusicLabels.searchAction)
            }
        }

        MSearchStatus(
            mAddMusicSheetState = mAddMusicSheetState,
            mAddMusicLabels = mAddMusicLabels,
            providerUnavailable = providerUnavailable,
            kindUnavailable = kindUnavailable,
            selectedProvider = selectedProvider,
            onCapabilitiesRetryClicked = onCapabilitiesRetryClicked,
            onSearchRetryClicked = onSearchRetryClicked
        )

        selectedResult?.let { result ->
            MSelectedResultCard(
                result = result,
                mAddMusicLabels = mAddMusicLabels,
                commandInFlight = mAddMusicSheetState.commandInFlight,
                onAddClicked = onSelectedResultAddClicked
            )
        }

        MResultList(
            mAddMusicSheetState = mAddMusicSheetState,
            mAddMusicLabels = mAddMusicLabels,
            onResultClicked = onResultClicked,
            onNextPageRequested = onNextPageRequested
        )
    }
}
