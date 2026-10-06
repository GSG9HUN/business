package com.dc.melodiasmario.feature.currenttrack.ui.components

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.clickable
import androidx.compose.foundation.interaction.MutableInteractionSource
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.heightIn
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.LazyRow
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.text.BasicTextField
import androidx.compose.material3.Button
import androidx.compose.material3.DropdownMenu
import androidx.compose.material3.DropdownMenuItem
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.IconButton
import androidx.compose.material3.ModalBottomSheet
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.TextStyle
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.dc.melodiasmario.core.common.TrackSearchPrefix
import com.dc.melodiasmario.core.commonui.components.MArtwork
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.components.input.MSearchBar
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeTokens
import com.dc.melodiasmario.core.commonui.formatter.toDurationLabel
import com.dc.melodiasmario.core.model.search.MusicSearchCapability
import com.dc.melodiasmario.core.model.search.MusicSearchResult
import com.dc.melodiasmario.core.model.search.MusicSearchResultKind
import com.dc.melodiasmario.feature.currenttrack.generated.resources.Res
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_add_music_manual_mode
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_add_music_mode_help
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_add_music_search_mode
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_add_to_queue_action
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_add_to_queue_placeholder
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_add_to_queue_title
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_cancel
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_search_action
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_search_add_playlist
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_search_add_track
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_search_capabilities_loading
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_search_empty
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_search_error
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_search_kind_unavailable
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_search_kind_help
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_search_load_more
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_search_placeholder
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_search_playlist_count
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_search_playlists
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_search_provider_label
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_search_provider_unavailable
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_search_retry
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_search_tracks
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_search_unknown_duration
import com.dc.melodiasmario.feature.currenttrack.presentation.addmusic.CurrentTrackAddMusicMode
import com.dc.melodiasmario.feature.currenttrack.presentation.state.CurrentTrackAddToQueueUiState
import com.dc.melodiasmario.feature.currenttrack.presentation.state.CurrentTrackSearchUiState
import org.jetbrains.compose.resources.stringResource

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun AddMusicSheet(
    searchState: CurrentTrackSearchUiState,
    addToQueueState: CurrentTrackAddToQueueUiState,
    commandInFlight: Boolean,
    onDismiss: () -> Unit,
    onModeChanged: (CurrentTrackAddMusicMode) -> Unit,
    onManualDraftChanged: (String) -> Unit,
    onManualConfirm: () -> Unit,
    onSearchQueryChanged: (String) -> Unit,
    onSearchSubmitted: () -> Unit,
    onProviderChanged: (TrackSearchPrefix) -> Unit,
    onKindChanged: (MusicSearchResultKind) -> Unit,
    onResultClicked: (MusicSearchResult) -> Unit,
    onSelectedResultAddClicked: () -> Unit,
    onNextPageRequested: () -> Unit,
    onSearchRetryClicked: () -> Unit,
    onCapabilitiesRetryClicked: () -> Unit,
) {
    ModalBottomSheet(onDismissRequest = onDismiss) {
        Column(
            modifier = Modifier
                .fillMaxWidth()
                .heightIn(max = 720.dp)
                .padding(horizontal = 16.dp)
                .padding(bottom = 24.dp),
            verticalArrangement = Arrangement.spacedBy(14.dp),
        ) {
            MText(
                modifier = Modifier.fillMaxWidth(),
                text = stringResource(Res.string.currenttrack_add_to_queue_title),
                color = MelodiasMarioThemeTokens.current.textPrimary,
                fontSize = 20.sp,
                fontWeight = FontWeight.Bold,
            )

            AddMusicModeSelector(
                selectedMode = searchState.mode,
                onModeChanged = onModeChanged,
            )

            when (searchState.mode) {
                CurrentTrackAddMusicMode.Search -> SearchContent(
                    searchState = searchState,
                    commandInFlight = commandInFlight,
                    onQueryChanged = onSearchQueryChanged,
                    onSearchSubmitted = onSearchSubmitted,
                    onProviderChanged = onProviderChanged,
                    onKindChanged = onKindChanged,
                    onResultClicked = onResultClicked,
                    onSelectedResultAddClicked = onSelectedResultAddClicked,
                    onNextPageRequested = onNextPageRequested,
                    onSearchRetryClicked = onSearchRetryClicked,
                    onCapabilitiesRetryClicked = onCapabilitiesRetryClicked,
                )

                CurrentTrackAddMusicMode.Manual -> ManualContent(
                    addToQueueState = addToQueueState,
                    commandInFlight = commandInFlight,
                    onDraftChanged = onManualDraftChanged,
                    onConfirm = onManualConfirm,
                    onDismiss = onDismiss,
                )
            }
        }
    }
}

@Composable
private fun AddMusicModeSelector(
    selectedMode: CurrentTrackAddMusicMode,
    onModeChanged: (CurrentTrackAddMusicMode) -> Unit,
) {
    Row(
        modifier = Modifier.fillMaxWidth(),
        verticalAlignment = Alignment.CenterVertically,
        horizontalArrangement = Arrangement.spacedBy(8.dp),
    ) {
        ModeButton(
            modifier = Modifier.weight(1f),
            text = stringResource(Res.string.currenttrack_add_music_search_mode),
            selected = selectedMode == CurrentTrackAddMusicMode.Search,
            onClick = { onModeChanged(CurrentTrackAddMusicMode.Search) },
        )
        ModeButton(
            modifier = Modifier.weight(1f),
            text = stringResource(Res.string.currenttrack_add_music_manual_mode),
            selected = selectedMode == CurrentTrackAddMusicMode.Manual,
            onClick = { onModeChanged(CurrentTrackAddMusicMode.Manual) },
        )
        HelpTooltipButton(text = stringResource(Res.string.currenttrack_add_music_mode_help))
    }
}

@Composable
private fun ModeButton(
    modifier: Modifier,
    text: String,
    selected: Boolean,
    onClick: () -> Unit,
) {
    if (selected) {
        Button(modifier = modifier, onClick = onClick) {
            MText(text = text, color = MelodiasMarioThemeTokens.current.textPrimary)
        }
    } else {
        OutlinedButton(modifier = modifier, onClick = onClick) {
            MText(text = text, color = MelodiasMarioThemeTokens.current.textSecondary)
        }
    }
}

@Composable
private fun SearchContent(
    searchState: CurrentTrackSearchUiState,
    commandInFlight: Boolean,
    onQueryChanged: (String) -> Unit,
    onSearchSubmitted: () -> Unit,
    onProviderChanged: (TrackSearchPrefix) -> Unit,
    onKindChanged: (MusicSearchResultKind) -> Unit,
    onResultClicked: (MusicSearchResult) -> Unit,
    onSelectedResultAddClicked: () -> Unit,
    onNextPageRequested: () -> Unit,
    onSearchRetryClicked: () -> Unit,
    onCapabilitiesRetryClicked: () -> Unit,
) {
    val capability = searchState.capabilityForSelectedProvider()
    val providerUnavailable = searchState.capabilities.isNotEmpty() &&
        (capability == null || !capability.canEnqueue)
    val kindUnavailable = capability != null && !capability.supports(searchState.selectedKind)
    val canSearch = !searchState.isCapabilitiesLoading &&
        !providerUnavailable &&
        !kindUnavailable

    Column(
        modifier = Modifier.fillMaxWidth(),
        verticalArrangement = Arrangement.spacedBy(12.dp),
    ) {
        ProviderSelector(
            selectedProvider = searchState.selectedProvider,
            capabilities = searchState.capabilities,
            onProviderChanged = onProviderChanged,
        )

        KindSelector(
            selectedKind = searchState.selectedKind,
            onKindChanged = onKindChanged,
        )

        Row(
            modifier = Modifier.fillMaxWidth(),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.spacedBy(8.dp),
        ) {
            MSearchBar(
                modifier = Modifier.weight(1f),
                query = searchState.query,
                onQueryChange = onQueryChanged,
                placeholder = stringResource(Res.string.currenttrack_search_placeholder),
                enabled = canSearch,
            )
            Button(
                onClick = onSearchSubmitted,
                enabled = canSearch && searchState.query.trim().length >= 2,
            ) {
                MText(text = stringResource(Res.string.currenttrack_search_action))
            }
        }

        SearchStatus(
            searchState = searchState,
            providerUnavailable = providerUnavailable,
            kindUnavailable = kindUnavailable,
            capability = capability,
            onCapabilitiesRetryClicked = onCapabilitiesRetryClicked,
            onSearchRetryClicked = onSearchRetryClicked,
        )

        searchState.selectedResult?.let { result ->
            SelectedResultCard(
                result = result,
                commandInFlight = commandInFlight,
                onAddClicked = onSelectedResultAddClicked,
            )
        }

        ResultList(
            searchState = searchState,
            onResultClicked = onResultClicked,
            onNextPageRequested = onNextPageRequested,
        )
    }
}

@Composable
private fun ProviderSelector(
    selectedProvider: TrackSearchPrefix,
    capabilities: List<MusicSearchCapability>,
    onProviderChanged: (TrackSearchPrefix) -> Unit,
) {
    var expanded by remember { mutableStateOf(false) }
    val colors = MelodiasMarioThemeTokens.current

    Box(modifier = Modifier.fillMaxWidth()) {
        OutlinedButton(
            modifier = Modifier.fillMaxWidth(),
            onClick = { expanded = true },
        ) {
            Row(
                modifier = Modifier.fillMaxWidth(),
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.spacedBy(8.dp),
            ) {
                MText(
                    modifier = Modifier.weight(1f),
                    text = "${stringResource(Res.string.currenttrack_search_provider_label)}: ${selectedProvider.displayName()}",
                    color = colors.textPrimary,
                    textAlign = TextAlign.Start,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis,
                )
                MText(text = "v", color = colors.textSecondary)
            }
        }

        DropdownMenu(
            expanded = expanded,
            onDismissRequest = { expanded = false },
        ) {
            TrackSearchPrefix.entries.forEach { provider ->
                val capability = capabilities.firstOrNull { it.provider == provider }
                val unavailable = capabilities.isNotEmpty() && (capability == null || !capability.canEnqueue)
                DropdownMenuItem(
                    text = {
                        MText(
                            text = provider.displayName(),
                            color = if (unavailable) colors.textMuted else colors.textPrimary,
                            textAlign = TextAlign.Start,
                        )
                    },
                    onClick = {
                        expanded = false
                        onProviderChanged(provider)
                    },
                )
            }
        }
    }
}

@Composable
private fun KindSelector(
    selectedKind: MusicSearchResultKind,
    onKindChanged: (MusicSearchResultKind) -> Unit,
) {
    Row(
        modifier = Modifier.fillMaxWidth(),
        verticalAlignment = Alignment.CenterVertically,
        horizontalArrangement = Arrangement.spacedBy(8.dp),
    ) {
        SelectablePill(
            modifier = Modifier.weight(1f),
            text = stringResource(Res.string.currenttrack_search_tracks),
            selected = selectedKind == MusicSearchResultKind.TRACK,
            onClick = { onKindChanged(MusicSearchResultKind.TRACK) },
        )
        SelectablePill(
            modifier = Modifier.weight(1f),
            text = stringResource(Res.string.currenttrack_search_playlists),
            selected = selectedKind == MusicSearchResultKind.PLAYLIST,
            onClick = { onKindChanged(MusicSearchResultKind.PLAYLIST) },
        )
        HelpTooltipButton(text = stringResource(Res.string.currenttrack_search_kind_help))
    }
}

@Composable
private fun HelpTooltipButton(
    text: String,
) {
    var expanded by remember { mutableStateOf(false) }
    val colors = MelodiasMarioThemeTokens.current

    Box {
        IconButton(onClick = { expanded = true }) {
            MText(
                text = "?",
                color = colors.textSecondary,
                fontWeight = FontWeight.Bold,
            )
        }
        DropdownMenu(
            expanded = expanded,
            onDismissRequest = { expanded = false },
        ) {
            MText(
                modifier = Modifier.padding(horizontal = 14.dp, vertical = 10.dp),
                text = text,
                color = colors.textPrimary,
                textAlign = TextAlign.Start,
            )
        }
    }
}

@Composable
private fun SelectablePill(
    text: String,
    selected: Boolean,
    onClick: () -> Unit,
    modifier: Modifier = Modifier,
    muted: Boolean = false,
) {
    val colors = MelodiasMarioThemeTokens.current
    Surface(
        modifier = modifier.clickable(onClick = onClick),
        shape = RoundedCornerShape(8.dp),
        color = if (selected) colors.primary else colors.elevated,
        border = BorderStroke(1.dp, if (selected) colors.primary else colors.outline),
    ) {
        MText(
            modifier = Modifier.padding(horizontal = 12.dp, vertical = 9.dp),
            text = text,
            color = when {
                selected -> colors.textPrimary
                muted -> colors.textMuted
                else -> colors.textSecondary
            },
            fontWeight = if (selected) FontWeight.Bold else null,
            maxLines = 1,
            overflow = TextOverflow.Ellipsis,
        )
    }
}

@Composable
private fun SearchStatus(
    searchState: CurrentTrackSearchUiState,
    providerUnavailable: Boolean,
    kindUnavailable: Boolean,
    capability: MusicSearchCapability?,
    onCapabilitiesRetryClicked: () -> Unit,
    onSearchRetryClicked: () -> Unit,
) {
    val message = when {
        searchState.isCapabilitiesLoading -> stringResource(Res.string.currenttrack_search_capabilities_loading)
        providerUnavailable -> capability?.unavailableReason
            ?: stringResource(Res.string.currenttrack_search_provider_unavailable)
        kindUnavailable -> stringResource(Res.string.currenttrack_search_kind_unavailable)
        searchState.errorMessage != null -> searchState.errorMessage
            ?: stringResource(Res.string.currenttrack_search_error)
        searchState.isSearching -> stringResource(Res.string.currenttrack_search_action)
        searchState.results.isEmpty() -> stringResource(Res.string.currenttrack_search_empty)
        else -> null
    }

    message?.let {
        Row(
            modifier = Modifier.fillMaxWidth(),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.spacedBy(8.dp),
        ) {
            MText(
                modifier = Modifier.weight(1f),
                text = it,
                color = MelodiasMarioThemeTokens.current.textMuted,
                textAlign = TextAlign.Start,
            )
            if (searchState.errorMessage != null) {
                OutlinedButton(onClick = onSearchRetryClicked) {
                    MText(text = stringResource(Res.string.currenttrack_search_retry))
                }
            } else if (providerUnavailable) {
                OutlinedButton(onClick = onCapabilitiesRetryClicked) {
                    MText(text = stringResource(Res.string.currenttrack_search_retry))
                }
            }
        }
    }
}

@Composable
private fun SelectedResultCard(
    result: MusicSearchResult,
    commandInFlight: Boolean,
    onAddClicked: () -> Unit,
) {
    val colors = MelodiasMarioThemeTokens.current
    Surface(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(8.dp),
        color = colors.surface,
        border = BorderStroke(1.dp, colors.primary),
    ) {
        Row(
            modifier = Modifier.fillMaxWidth().padding(10.dp),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.spacedBy(10.dp),
        ) {
            SearchArtwork(result.thumbnailUrl)
            Column(modifier = Modifier.weight(1f)) {
                MText(
                    modifier = Modifier.fillMaxWidth(),
                    text = result.title,
                    color = colors.textPrimary,
                    textAlign = TextAlign.Start,
                    fontWeight = FontWeight.Bold,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis,
                )
                MText(
                    modifier = Modifier.fillMaxWidth(),
                    text = result.subtitle(),
                    color = colors.textMuted,
                    textAlign = TextAlign.Start,
                    fontSize = 12.sp,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis,
                )
            }
            Button(
                onClick = onAddClicked,
                enabled = result.canEnqueue && result.canonicalUrl != null && !commandInFlight,
            ) {
                MText(
                    text = stringResource(
                        if (result.kind == MusicSearchResultKind.PLAYLIST) {
                            Res.string.currenttrack_search_add_playlist
                        } else {
                            Res.string.currenttrack_search_add_track
                        }
                    )
                )
            }
        }
    }
}

@Composable
private fun ResultList(
    searchState: CurrentTrackSearchUiState,
    onResultClicked: (MusicSearchResult) -> Unit,
    onNextPageRequested: () -> Unit,
) {
    LazyColumn(
        modifier = Modifier.fillMaxWidth().heightIn(max = 320.dp),
        verticalArrangement = Arrangement.spacedBy(8.dp),
    ) {
        items(searchState.results, key = { it.id }) { result ->
            SearchResultRow(
                result = result,
                selected = searchState.selectedResult?.id == result.id,
                onClick = { onResultClicked(result) },
            )
        }

        if (searchState.nextPageToken != null) {
            item {
                OutlinedButton(
                    modifier = Modifier.fillMaxWidth(),
                    onClick = onNextPageRequested,
                    enabled = !searchState.isLoadingNextPage && !searchState.isSearching,
                ) {
                    MText(text = stringResource(Res.string.currenttrack_search_load_more))
                }
            }
        }
    }
}

@Composable
private fun SearchResultRow(
    result: MusicSearchResult,
    selected: Boolean,
    onClick: () -> Unit,
) {
    val colors = MelodiasMarioThemeTokens.current
    Surface(
        modifier = Modifier.fillMaxWidth().clickable(onClick = onClick),
        shape = RoundedCornerShape(8.dp),
        color = if (selected) colors.elevated else colors.surface,
        border = BorderStroke(1.dp, if (selected) colors.primary else colors.outline),
    ) {
        Row(
            modifier = Modifier.fillMaxWidth().padding(10.dp),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.spacedBy(10.dp),
        ) {
            SearchArtwork(result.thumbnailUrl)
            Column(modifier = Modifier.weight(1f)) {
                MText(
                    modifier = Modifier.fillMaxWidth(),
                    text = result.title,
                    color = colors.textPrimary,
                    textAlign = TextAlign.Start,
                    fontWeight = FontWeight.Bold,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis,
                )
                MText(
                    modifier = Modifier.fillMaxWidth(),
                    text = result.subtitle(),
                    color = colors.textMuted,
                    textAlign = TextAlign.Start,
                    fontSize = 12.sp,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis,
                )
            }
        }
    }
}

@Composable
private fun SearchArtwork(imageUrl: String?) {
    MArtwork(
        modifier = Modifier.size(44.dp),
        iconSize = 22.dp,
        imageUrl = imageUrl,
    )
}

@Composable
private fun ManualContent(
    addToQueueState: CurrentTrackAddToQueueUiState,
    commandInFlight: Boolean,
    onDraftChanged: (String) -> Unit,
    onConfirm: () -> Unit,
    onDismiss: () -> Unit,
) {
    Column(
        modifier = Modifier.fillMaxWidth(),
        verticalArrangement = Arrangement.spacedBy(12.dp),
    ) {
        ManualInput(
            value = addToQueueState.draft,
            onValueChange = onDraftChanged,
            enabled = !commandInFlight && !addToQueueState.isLoading,
        )
        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.spacedBy(8.dp),
        ) {
            OutlinedButton(
                modifier = Modifier.weight(1f),
                onClick = onDismiss,
            ) {
                MText(text = stringResource(Res.string.currenttrack_cancel))
            }
            Button(
                modifier = Modifier.weight(1f),
                onClick = onConfirm,
                enabled = !commandInFlight && !addToQueueState.isLoading && addToQueueState.canSubmit,
            ) {
                MText(text = stringResource(Res.string.currenttrack_add_to_queue_action))
            }
        }
    }
}

@Composable
private fun ManualInput(
    value: String,
    onValueChange: (String) -> Unit,
    enabled: Boolean,
) {
    val colors = MelodiasMarioThemeTokens.current
    Surface(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(12.dp),
        color = colors.elevated,
        border = BorderStroke(1.dp, colors.outline),
    ) {
        BasicTextField(
            value = value,
            onValueChange = onValueChange,
            enabled = enabled,
            singleLine = true,
            textStyle = TextStyle(color = colors.textPrimary),
            interactionSource = remember { MutableInteractionSource() },
            modifier = Modifier.fillMaxWidth().padding(horizontal = 14.dp, vertical = 13.dp),
            decorationBox = { innerTextField ->
                Box(
                    modifier = Modifier.fillMaxWidth(),
                    contentAlignment = Alignment.CenterStart,
                ) {
                    if (value.isBlank()) {
                        MText(
                            text = stringResource(Res.string.currenttrack_add_to_queue_placeholder),
                            color = colors.textMuted,
                            textAlign = TextAlign.Start,
                        )
                    }
                    innerTextField()
                }
            },
        )
    }
}

private fun CurrentTrackSearchUiState.capabilityForSelectedProvider(): MusicSearchCapability? {
    return capabilities.firstOrNull { it.provider == selectedProvider }
}

private fun MusicSearchCapability.supports(kind: MusicSearchResultKind): Boolean {
    return when (kind) {
        MusicSearchResultKind.TRACK -> supportsTrackSearch
        MusicSearchResultKind.PLAYLIST -> supportsPlaylistSearch
    }
}

private fun TrackSearchPrefix.displayName(): String {
    return when (this) {
        TrackSearchPrefix.SPOTIFY -> "Spotify"
        TrackSearchPrefix.SOUNDCLOUD -> "SoundCloud"
        TrackSearchPrefix.YOUTUBE_MUSIC -> "YouTube Music"
        TrackSearchPrefix.YOUTUBE -> "YouTube"
        TrackSearchPrefix.APPLE_MUSIC -> "Apple Music"
        TrackSearchPrefix.DEEZER -> "Deezer"
        TrackSearchPrefix.YANDEX_MUSIC -> "Yandex Music"
        TrackSearchPrefix.BANDCAMP -> "Bandcamp"
    }
}

@Composable
private fun MusicSearchResult.subtitle(): String {
    val provider = provider.displayName()
    val playlistItemCount = itemCount
    val trackDurationSeconds = durationSeconds
    val detail = when {
        kind == MusicSearchResultKind.PLAYLIST && playlistItemCount != null ->
            stringResource(Res.string.currenttrack_search_playlist_count, playlistItemCount)
        trackDurationSeconds != null -> trackDurationSeconds.toDurationLabel()
        kind == MusicSearchResultKind.TRACK -> stringResource(Res.string.currenttrack_search_unknown_duration)
        else -> null
    }
    return listOfNotNull(creator, provider, detail).joinToString(" - ")
}
