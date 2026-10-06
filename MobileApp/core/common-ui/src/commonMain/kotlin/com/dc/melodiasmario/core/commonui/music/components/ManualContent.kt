package com.dc.melodiasmario.core.commonui.music.components

import androidx.compose.runtime.Composable
import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.music.model.MAddMusicLabels
import com.dc.melodiasmario.core.commonui.music.state.MAddMusicSheetState

@Composable
fun MManualContent(
    state: MAddMusicSheetState,
    labels: MAddMusicLabels,
    onDraftChanged: (String) -> Unit,
    onConfirm: () -> Unit,
    onDismiss: () -> Unit,
) {
    Column(
        modifier = Modifier.fillMaxWidth(),
        verticalArrangement = Arrangement.spacedBy(12.dp),
    ) {
        MManualInput(
            value = state.manualDraft,
            placeholder = labels.manualPlaceholder,
            onValueChange = onDraftChanged,
            enabled = !state.commandInFlight && !state.isManualLoading,
        )
        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.spacedBy(8.dp),
        ) {
            OutlinedButton(
                modifier = Modifier.weight(1f),
                onClick = onDismiss,
            ) {
                MText(text = labels.manualCancel)
            }
            Button(
                modifier = Modifier.weight(1f),
                onClick = onConfirm,
                enabled = !state.commandInFlight && !state.isManualLoading && state.canSubmitManual,
            ) {
                MText(text = labels.manualAdd)
            }
        }
    }
}