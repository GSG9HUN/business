package com.dc.melodiasmario.core.commonui.music.components

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.WindowInsets
import androidx.compose.foundation.layout.fillMaxHeight
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.heightIn
import androidx.compose.foundation.layout.ime
import androidx.compose.foundation.layout.imePadding
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.ModalBottomSheet
import androidx.compose.material3.rememberModalBottomSheetState
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalDensity
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.music.model.MAddMusicLabels
import com.dc.melodiasmario.core.presentation.music.model.MAddMusicMode
import com.dc.melodiasmario.core.presentation.music.model.MMusicSearchKind
import com.dc.melodiasmario.core.presentation.music.state.MAddMusicSheetState

/**
 * Renders add music sheet from the supplied parameters and forwards interaction callbacks.
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun MAddMusicSheet(
    mAddMusicSheetState: MAddMusicSheetState,
    mAddMusicLabels: MAddMusicLabels,
    onDismiss: () -> Unit,
    onModeChanged: (addMusicMode: MAddMusicMode) -> Unit,
    onManualDraftChanged: (manualDraft: String) -> Unit,
    onManualConfirm: () -> Unit,
    onSearchQueryChanged: (searchQuery: String) -> Unit,
    onSearchSubmitted: () -> Unit,
    onProviderChanged: (providerId: String) -> Unit,
    onKindChanged: (musicSearchKind: MMusicSearchKind) -> Unit,
    onResultClicked: (resultId: String) -> Unit,
    onSelectedResultAddClicked: () -> Unit,
    onNextPageRequested: () -> Unit,
    onSearchRetryClicked: () -> Unit,
    onCapabilitiesRetryClicked: () -> Unit
): Unit {
    val sheetState = rememberModalBottomSheetState(skipPartiallyExpanded = true)
    val density = LocalDensity.current
    val imeBottom = WindowInsets.ime.getBottom(density)

    LaunchedEffect(key1 = Unit) {
        sheetState.expand()
    }

    LaunchedEffect(key1 = imeBottom) {
        if (imeBottom > 0) {
            sheetState.expand()
        }
    }

    ModalBottomSheet(
        onDismissRequest = onDismiss,
        sheetState = sheetState
    ) {
        Column(
            modifier = Modifier
                .fillMaxWidth()
                .fillMaxHeight(fraction = 0.92f)
                .heightIn(max = 720.dp)
                .imePadding()
                .padding(horizontal = 16.dp)
                .padding(bottom = 24.dp),
            verticalArrangement = Arrangement.spacedBy(space = 14.dp)
        ) {
            MText(
                modifier = Modifier.fillMaxWidth(),
                text = mAddMusicLabels.title,
                color = MaterialTheme.colorScheme.onSurface,
                fontSize = 20.sp,
                fontWeight = FontWeight.Bold
            )

            MAddMusicModeSelector(
                mAddMusicLabels = mAddMusicLabels,
                selectedMode = mAddMusicSheetState.mode,
                onModeChanged = onModeChanged
            )

            when (mAddMusicSheetState.mode) {
                MAddMusicMode.Search -> MSearchContent(
                    mAddMusicSheetState = mAddMusicSheetState,
                    mAddMusicLabels = mAddMusicLabels,
                    onQueryChanged = onSearchQueryChanged,
                    onSearchSubmitted = onSearchSubmitted,
                    onProviderChanged = onProviderChanged,
                    onKindChanged = onKindChanged,
                    onResultClicked = onResultClicked,
                    onSelectedResultAddClicked = onSelectedResultAddClicked,
                    onNextPageRequested = onNextPageRequested,
                    onSearchRetryClicked = onSearchRetryClicked,
                    onCapabilitiesRetryClicked = onCapabilitiesRetryClicked
                )

                MAddMusicMode.Manual -> MManualContent(
                    mAddMusicSheetState = mAddMusicSheetState,
                    mAddMusicLabels = mAddMusicLabels,
                    onDraftChanged = onManualDraftChanged,
                    onConfirm = onManualConfirm,
                    onDismiss = onDismiss
                )
            }
        }
    }
}
