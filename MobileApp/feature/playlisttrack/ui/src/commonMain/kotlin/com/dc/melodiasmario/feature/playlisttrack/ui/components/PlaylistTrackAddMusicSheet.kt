package com.dc.melodiasmario.feature.playlisttrack.ui.components

import androidx.compose.runtime.Composable
import com.dc.melodiasmario.core.commonui.music.components.MAddMusicSheet
import com.dc.melodiasmario.core.commonui.music.components.rememberMAddMusicLabels
import com.dc.melodiasmario.core.presentation.music.state.MAddMusicSheetState
import com.dc.melodiasmario.feature.playlisttrack.presentation.PlaylistTrackEvent

/**
 * Renders playlist track add music sheet from the supplied parameters and forwards interaction callbacks.
 */
@Composable
internal fun PlaylistTrackAddMusicSheet(
    mAddMusicSheetState: MAddMusicSheetState,
    onEvent: (playlistTrackEvent: PlaylistTrackEvent) -> Unit
): Unit {
    MAddMusicSheet(
        mAddMusicSheetState = mAddMusicSheetState,
        mAddMusicLabels = rememberMAddMusicLabels(),
        onDismiss = { onEvent(PlaylistTrackEvent.DismissDialog) },
        onModeChanged = { addMusicMode -> onEvent(PlaylistTrackEvent.ModeChanged(mode = addMusicMode)) },
        onManualDraftChanged = { manualDraft -> onEvent(PlaylistTrackEvent.ManualDraftChanged(value = manualDraft)) },
        onManualConfirm = { onEvent(PlaylistTrackEvent.ManualAdd) },
        onSearchQueryChanged = { searchQuery -> onEvent(PlaylistTrackEvent.SearchQueryChanged(value = searchQuery)) },
        onSearchSubmitted = { onEvent(PlaylistTrackEvent.SearchSubmitted) },
        onProviderChanged = { providerId -> onEvent(PlaylistTrackEvent.ProviderChanged(id = providerId)) },
        onKindChanged = { musicSearchKind -> onEvent(PlaylistTrackEvent.KindChanged(kind = musicSearchKind)) },
        onResultClicked = { searchResultId -> onEvent(PlaylistTrackEvent.ResultClicked(id = searchResultId)) },
        onSelectedResultAddClicked = { onEvent(PlaylistTrackEvent.AddSelectedResult) },
        onNextPageRequested = { onEvent(PlaylistTrackEvent.NextPage) },
        onSearchRetryClicked = { onEvent(PlaylistTrackEvent.RetrySearch) },
        onCapabilitiesRetryClicked = { onEvent(PlaylistTrackEvent.RetryCapabilities) }
    )
}
