package com.dc.melodiasmario.core.commonui.music.components

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.material3.Button
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.components.input.MSearchBar
import com.dc.melodiasmario.core.commonui.music.model.MAddMusicLabels
import com.dc.melodiasmario.core.commonui.music.model.MMusicSearchKind
import com.dc.melodiasmario.core.commonui.music.state.MAddMusicSheetState

@Composable
fun MSearchContent(
    state: MAddMusicSheetState,
    labels: MAddMusicLabels,
    onQueryChanged: (String) -> Unit,
    onSearchSubmitted: () -> Unit,
    onProviderChanged: (String) -> Unit,
    onKindChanged: (MMusicSearchKind) -> Unit,
    onResultClicked: (String) -> Unit,
    onSelectedResultAddClicked: () -> Unit,
    onNextPageRequested: () -> Unit,
    onSearchRetryClicked: () -> Unit,
    onCapabilitiesRetryClicked: () -> Unit,
) {
    val selectedProvider = state.providers.firstOrNull { it.id == state.selectedProviderId }
    val providerUnavailable = state.providers.isNotEmpty() &&
            (selectedProvider == null || !selectedProvider.canEnqueue)
    val kindUnavailable = selectedProvider != null && when (state.selectedKind) {
        MMusicSearchKind.Track -> !selectedProvider.supportsTrackSearch
        MMusicSearchKind.Playlist -> !selectedProvider.supportsPlaylistSearch
    }
    val canSearch = !state.isCapabilitiesLoading && !providerUnavailable && !kindUnavailable
    val selectedResult = state.results.firstOrNull { it.id == state.selectedResultId }

    Column(
        modifier = Modifier.fillMaxWidth(),
        verticalArrangement = Arrangement.spacedBy(12.dp),
    ) {
        MProviderSelector(
            label = labels.providerLabel,
            selectedProvider = selectedProvider,
            providers = state.providers,
            onProviderChanged = onProviderChanged,
        )

        MKindSelector(
            labels = labels,
            selectedKind = state.selectedKind,
            onKindChanged = onKindChanged,
        )

        Row(
            modifier = Modifier.fillMaxWidth(),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.spacedBy(8.dp),
        ) {
            MSearchBar(
                modifier = Modifier.weight(1f),
                query = state.query,
                onQueryChange = onQueryChanged,
                placeholder = labels.searchPlaceholder,
                enabled = canSearch,
            )
            Button(
                onClick = onSearchSubmitted,
                enabled = canSearch && state.query.trim().length >= 2,
            ) {
                MText(text = labels.searchAction)
            }
        }

        MSearchStatus(
            state = state,
            labels = labels,
            providerUnavailable = providerUnavailable,
            kindUnavailable = kindUnavailable,
            selectedProvider = selectedProvider,
            onCapabilitiesRetryClicked = onCapabilitiesRetryClicked,
            onSearchRetryClicked = onSearchRetryClicked,
        )

        selectedResult?.let { result ->
            MSelectedResultCard(
                result = result,
                labels = labels,
                commandInFlight = state.commandInFlight,
                onAddClicked = onSelectedResultAddClicked,
            )
        }

        MResultList(
            state = state,
            labels = labels,
            onResultClicked = onResultClicked,
            onNextPageRequested = onNextPageRequested,
        )
    }
}