package com.dc.melodiasmario.core.commonui.music.components

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.material3.Button
import androidx.compose.material3.OutlinedButton
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalFocusManager
import androidx.compose.ui.platform.LocalSoftwareKeyboardController
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.music.model.MAddMusicLabels
import com.dc.melodiasmario.core.presentation.music.state.MAddMusicSheetState

/**
 * Renders manual content from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun MManualContent(
    mAddMusicSheetState: MAddMusicSheetState,
    mAddMusicLabels: MAddMusicLabels,
    onDraftChanged: (manualDraft: String) -> Unit,
    onConfirm: () -> Unit,
    onDismiss: () -> Unit
): Unit {
    val focusManager = LocalFocusManager.current
    val keyboardController = LocalSoftwareKeyboardController.current
    fun confirm() {
        keyboardController?.hide()
        focusManager.clearFocus()
        onConfirm()
    }

    Column(
        modifier = Modifier.fillMaxWidth(),
        verticalArrangement = Arrangement.spacedBy(space = 12.dp)
    ) {
        MManualInput(
            value = mAddMusicSheetState.manualDraft,
            placeholder = mAddMusicLabels.manualPlaceholder,
            onValueChange = onDraftChanged,
            enabled = !mAddMusicSheetState.commandInFlight && !mAddMusicSheetState.isManualLoading
        )
        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.spacedBy(space = 8.dp)
        ) {
            OutlinedButton(
                modifier = Modifier.weight(weight = 1f),
                onClick = onDismiss
            ) {
                MText(text = mAddMusicLabels.manualCancel)
            }
            Button(
                modifier = Modifier.weight(weight = 1f),
                onClick = ::confirm,
                enabled = !mAddMusicSheetState.commandInFlight && !mAddMusicSheetState.isManualLoading && mAddMusicSheetState.canSubmitManual
            ) {
                MText(text = mAddMusicLabels.manualAdd)
            }
        }
    }
}
