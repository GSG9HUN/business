package com.dc.melodiasmario.feature.playlisttrack.ui.components

import androidx.compose.runtime.Composable
import com.dc.melodiasmario.core.commonui.music.components.MAddMusicSheet
import com.dc.melodiasmario.core.commonui.music.components.rememberMAddMusicLabels
import com.dc.melodiasmario.core.presentation.music.state.MAddMusicSheetState
import com.dc.melodiasmario.feature.playlisttrack.presentation.PlaylistTrackEvent

@Composable
internal fun PlaylistTrackAddMusicSheet(
    state: MAddMusicSheetState,
    onEvent: (PlaylistTrackEvent) -> Unit,
) {
    MAddMusicSheet(
        state = state,
        labels = rememberMAddMusicLabels(),
        onDismiss = { onEvent(PlaylistTrackEvent.DismissDialog) },
        onModeChanged = { onEvent(PlaylistTrackEvent.ModeChanged(it)) },
        onManualDraftChanged = { onEvent(PlaylistTrackEvent.ManualDraftChanged(it)) },
        onManualConfirm = { onEvent(PlaylistTrackEvent.ManualAdd) },
        onSearchQueryChanged = { onEvent(PlaylistTrackEvent.SearchQueryChanged(it)) },
        onSearchSubmitted = { onEvent(PlaylistTrackEvent.SearchSubmitted) },
        onProviderChanged = { onEvent(PlaylistTrackEvent.ProviderChanged(it)) },
        onKindChanged = { onEvent(PlaylistTrackEvent.KindChanged(it)) },
        onResultClicked = { onEvent(PlaylistTrackEvent.ResultClicked(it)) },
        onSelectedResultAddClicked = { onEvent(PlaylistTrackEvent.AddSelectedResult) },
        onNextPageRequested = { onEvent(PlaylistTrackEvent.NextPage) },
        onSearchRetryClicked = { onEvent(PlaylistTrackEvent.RetrySearch) },
        onCapabilitiesRetryClicked = { onEvent(PlaylistTrackEvent.RetryCapabilities) },
    )
}
