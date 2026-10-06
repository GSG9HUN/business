package com.dc.melodiasmario.core.commonui.music.components

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.heightIn
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.ModalBottomSheet
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeTokens
import com.dc.melodiasmario.core.commonui.music.model.MAddMusicLabels
import com.dc.melodiasmario.core.commonui.music.model.MAddMusicMode
import com.dc.melodiasmario.core.commonui.music.model.MMusicSearchKind
import com.dc.melodiasmario.core.commonui.music.state.MAddMusicSheetState

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun MAddMusicSheet(
    state: MAddMusicSheetState,
    labels: MAddMusicLabels,
    onDismiss: () -> Unit,
    onModeChanged: (MAddMusicMode) -> Unit,
    onManualDraftChanged: (String) -> Unit,
    onManualConfirm: () -> Unit,
    onSearchQueryChanged: (String) -> Unit,
    onSearchSubmitted: () -> Unit,
    onProviderChanged: (String) -> Unit,
    onKindChanged: (MMusicSearchKind) -> Unit,
    onResultClicked: (String) -> Unit,
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
                text = labels.title,
                color = MelodiasMarioThemeTokens.current.textPrimary,
                fontSize = 20.sp,
                fontWeight = FontWeight.Bold,
            )

            MAddMusicModeSelector(
                labels = labels,
                selectedMode = state.mode,
                onModeChanged = onModeChanged,
            )

            when (state.mode) {
                MAddMusicMode.Search -> MSearchContent(
                    state = state,
                    labels = labels,
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

                MAddMusicMode.Manual -> MManualContent(
                    state = state,
                    labels = labels,
                    onDraftChanged = onManualDraftChanged,
                    onConfirm = onManualConfirm,
                    onDismiss = onDismiss,
                )
            }
        }
    }
}
